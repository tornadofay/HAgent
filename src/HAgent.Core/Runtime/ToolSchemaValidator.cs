using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class ToolValidationResult
    {
        public bool IsValid { get; private set; }
        public IReadOnlyDictionary<string, object> Arguments { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; }

        private ToolValidationResult(bool valid, IReadOnlyDictionary<string, object> arguments, IReadOnlyList<string> errors)
        {
            IsValid = valid;
            Arguments = arguments ?? new Dictionary<string, object>();
            Errors = errors ?? new List<string>();
        }

        public static ToolValidationResult Success(IReadOnlyDictionary<string, object> arguments)
        {
            return new ToolValidationResult(true, arguments, new List<string>().AsReadOnly());
        }

        public static ToolValidationResult Failure(IEnumerable<string> errors)
        {
            return new ToolValidationResult(false, new Dictionary<string, object>(), new List<string>(errors ?? new string[0]).AsReadOnly());
        }
    }

    public static class ToolSchemaValidator
    {
        public static ToolValidationResult ValidateSchema(AiTool definition)
        {
            if (definition == null)
                return ToolValidationResult.Failure(new[] { "Tool definition is required." });

            if (string.IsNullOrWhiteSpace(definition.InputSchemaJson))
                return ToolValidationResult.Success(new Dictionary<string, object>());

            try
            {
                var schema = ParseObject(definition.InputSchemaJson);
                var errors = new List<string>();

                string type;
                if (TryGetString(schema, "type", out type) && !string.Equals(type, "object", StringComparison.OrdinalIgnoreCase))
                    errors.Add("$: tool input schema root must use type 'object'.");

                object rawProperties;
                if (schema.TryGetValue("properties", out rawProperties) && rawProperties != null && !(rawProperties is Dictionary<string, object>))
                    errors.Add("$.properties: must be a JSON object when provided.");

                object rawRequired;
                if (schema.TryGetValue("required", out rawRequired) && rawRequired != null)
                {
                    var required = rawRequired as List<object>;
                    if (required == null)
                        errors.Add("$.required: must be a JSON array when provided.");
                    else
                    {
                        for (var i = 0; i < required.Count; i++)
                            if (!(required[i] is string) || string.IsNullOrWhiteSpace((string)required[i]))
                                errors.Add("$.required[" + i + "]: must contain a non-empty property name.");
                    }
                }

                if (errors.Count > 0)
                    return ToolValidationResult.Failure(errors);

                return ToolValidationResult.Success(new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                return ToolValidationResult.Failure(new[] { "Tool input schema is invalid JSON: " + ex.Message });
            }
        }

        public static ToolValidationResult Validate(AiTool definition, IDictionary<string, object> arguments)
        {
            var schemaResult = ValidateSchema(definition);
            if (!schemaResult.IsValid) return schemaResult;

            Dictionary<string, object> schema;
            try
            {
                schema = ParseObject(definition.InputSchemaJson);
            }
            catch (Exception ex)
            {
                return ToolValidationResult.Failure(new[] { "Tool input schema is invalid JSON: " + ex.Message });
            }

            var source = new Dictionary<string, object>(arguments ?? new Dictionary<string, object>(), StringComparer.OrdinalIgnoreCase);
            var errors = new List<string>();
            ValidateObject(schema, source, "$", errors);
            if (errors.Count > 0) return ToolValidationResult.Failure(errors);
            return ToolValidationResult.Success(new Dictionary<string, object>(source, StringComparer.OrdinalIgnoreCase));
        }

        private static void ValidateObject(Dictionary<string, object> schema, IDictionary<string, object> args, string path, ICollection<string> errors)
        {
            string type;
            if (TryGetString(schema, "type", out type) && !string.Equals(type, "object", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(path + ": root tool arguments must use type 'object'.");
                return;
            }

            var required = GetStringList(schema, "required");
            for (var i = 0; i < required.Count; i++)
            {
                if (!args.ContainsKey(required[i]) || args[required[i]] == null)
                    errors.Add(path + "." + required[i] + ": required argument is missing.");
            }

            Dictionary<string, object> properties;
            schema.TryGetValue("properties", out var rawProperties);
            properties = rawProperties as Dictionary<string, object>;
            if (properties == null) return;

            var allowAdditional = true;
            object additional;
            if (schema.TryGetValue("additionalProperties", out additional) && additional is bool)
                allowAdditional = (bool)additional;

            foreach (var pair in args)
            {
                object rawProperty;
                if (!properties.TryGetValue(pair.Key, out rawProperty))
                {
                    Dictionary<string, object> caseInsensitiveProperty = null;
                    foreach (var candidate in properties)
                    {
                        if (string.Equals(candidate.Key, pair.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            caseInsensitiveProperty = candidate.Value as Dictionary<string, object>;
                            break;
                        }
                    }
                    if (caseInsensitiveProperty == null)
                    {
                        if (!allowAdditional) errors.Add(path + "." + pair.Key + ": argument is not allowed by the schema.");
                        continue;
                    }
                    ValidateValue(caseInsensitiveProperty, pair.Value, path + "." + pair.Key, errors);
                }
                else
                {
                    ValidateValue(rawProperty as Dictionary<string, object>, pair.Value, path + "." + pair.Key, errors);
                }
            }
        }

        private static void ValidateValue(Dictionary<string, object> schema, object value, string path, ICollection<string> errors)
        {
            if (schema == null || value == null) return;

            string expected;
            if (TryGetString(schema, "type", out expected))
            {
                switch (expected)
                {
                    case "string":
                        if (!(value is string)) errors.Add(path + ": expected a string.");
                        break;
                    case "integer":
                        if (!IsInteger(value)) errors.Add(path + ": expected an integer.");
                        break;
                    case "number":
                        if (!IsNumber(value)) errors.Add(path + ": expected a number.");
                        break;
                    case "boolean":
                        if (!(value is bool)) errors.Add(path + ": expected a boolean.");
                        break;
                    case "array":
                        if (!(value is IEnumerable) || value is string) errors.Add(path + ": expected an array.");
                        break;
                    case "object":
                        if (!(value is IDictionary<string, object>) && !(value is IDictionary<string, string>) && !(value is Dictionary<string, object>))
                            errors.Add(path + ": expected an object.");
                        break;
                }
            }

            List<object> enumValues;
            if (TryGetList(schema, "enum", out enumValues) && enumValues.Count > 0)
            {
                var matched = false;
                for (var i = 0; i < enumValues.Count; i++)
                {
                    if (string.Equals(Convert.ToString(enumValues[i], CultureInfo.InvariantCulture), Convert.ToString(value, CultureInfo.InvariantCulture), StringComparison.Ordinal))
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched) errors.Add(path + ": value is not one of the permitted enum values.");
            }

            string pattern;
            if (value is string && TryGetString(schema, "pattern", out pattern) && !string.IsNullOrWhiteSpace(pattern))
            {
                try
                {
                    if (!Regex.IsMatch((string)value, pattern)) errors.Add(path + ": string does not match the required pattern.");
                }
                catch (ArgumentException)
                {
                    errors.Add(path + ": schema contains an invalid regex pattern.");
                }
            }
        }

        private static bool IsInteger(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong;
        }

        private static bool IsNumber(object value)
        {
            return IsInteger(value) || value is float || value is double || value is decimal;
        }

        private static bool TryGetString(Dictionary<string, object> dictionary, string key, out string value)
        {
            object raw;
            if (dictionary.TryGetValue(key, out raw) && raw is string)
            {
                value = (string)raw;
                return true;
            }
            value = null;
            return false;
        }

        private static List<string> GetStringList(Dictionary<string, object> dictionary, string key)
        {
            object raw;
            var result = new List<string>();
            if (!dictionary.TryGetValue(key, out raw)) return result;
            var list = raw as List<object>;
            if (list == null) return result;
            for (var i = 0; i < list.Count; i++)
                if (list[i] is string) result.Add((string)list[i]);
            return result;
        }

        private static bool TryGetList(Dictionary<string, object> dictionary, string key, out List<object> list)
        {
            object raw;
            if (dictionary.TryGetValue(key, out raw))
            {
                list = raw as List<object>;
                return list != null;
            }
            list = null;
            return false;
        }

        private static Dictionary<string, object> ParseObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var parser = new MiniJsonParser(json);
            var value = parser.ParseValue();
            parser.SkipWhiteSpace();
            if (!parser.IsEnd) throw new FormatException("Unexpected characters after the JSON value.");
            var obj = value as Dictionary<string, object>;
            if (obj == null) throw new FormatException("Tool schema root must be a JSON object.");
            return obj;
        }

        private sealed class MiniJsonParser
        {
            private readonly string _json;
            private int _index;
            public MiniJsonParser(string json) { _json = json; }
            public bool IsEnd { get { return _index >= _json.Length; } }

            public object ParseValue()
            {
                SkipWhiteSpace();
                if (IsEnd) throw new FormatException("Unexpected end of JSON.");
                switch (_json[_index])
                {
                    case '{': return ParseObjectValue();
                    case '[': return ParseArrayValue();
                    case '"': return ParseString();
                    case 't': ParseLiteral("true"); return true;
                    case 'f': ParseLiteral("false"); return false;
                    case 'n': ParseLiteral("null"); return null;
                    default: return ParseNumber();
                }
            }

            public void SkipWhiteSpace()
            {
                while (!IsEnd && char.IsWhiteSpace(_json[_index])) _index++;
            }

            private Dictionary<string, object> ParseObjectValue()
            {
                var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _index++;
                SkipWhiteSpace();
                if (!IsEnd && _json[_index] == '}') { _index++; return result; }
                while (true)
                {
                    SkipWhiteSpace();
                    if (IsEnd || _json[_index] != '"') throw new FormatException("Expected a JSON property name.");
                    var name = ParseString();
                    SkipWhiteSpace();
                    Expect(':');
                    result[name] = ParseValue();
                    SkipWhiteSpace();
                    if (!IsEnd && _json[_index] == '}') { _index++; return result; }
                    Expect(',');
                }
            }

            private List<object> ParseArrayValue()
            {
                var result = new List<object>();
                _index++;
                SkipWhiteSpace();
                if (!IsEnd && _json[_index] == ']') { _index++; return result; }
                while (true)
                {
                    result.Add(ParseValue());
                    SkipWhiteSpace();
                    if (!IsEnd && _json[_index] == ']') { _index++; return result; }
                    Expect(',');
                }
            }

            private string ParseString()
            {
                Expect('"');
                var result = new System.Text.StringBuilder();
                while (!IsEnd)
                {
                    var c = _json[_index++];
                    if (c == '"') return result.ToString();
                    if (c != '\\') { result.Append(c); continue; }
                    if (IsEnd) throw new FormatException("Invalid JSON escape sequence.");
                    c = _json[_index++];
                    switch (c)
                    {
                        case '"': result.Append('"'); break;
                        case '\\': result.Append('\\'); break;
                        case '/': result.Append('/'); break;
                        case 'b': result.Append('\b'); break;
                        case 'f': result.Append('\f'); break;
                        case 'n': result.Append('\n'); break;
                        case 'r': result.Append('\r'); break;
                        case 't': result.Append('\t'); break;
                        case 'u':
                            if (_index + 4 > _json.Length) throw new FormatException("Invalid unicode escape sequence.");
                            result.Append((char)int.Parse(_json.Substring(_index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _index += 4;
                            break;
                        default: throw new FormatException("Invalid JSON escape sequence.");
                    }
                }
                throw new FormatException("Unterminated JSON string.");
            }

            private object ParseNumber()
            {
                var start = _index;
                while (!IsEnd && "-+0123456789.eE".IndexOf(_json[_index]) >= 0) _index++;
                var token = _json.Substring(start, _index - start);
                decimal decimalValue;
                if (decimal.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out decimalValue)) return decimalValue;
                throw new FormatException("Invalid JSON number.");
            }

            private void ParseLiteral(string literal)
            {
                if (_index + literal.Length > _json.Length || !string.Equals(_json.Substring(_index, literal.Length), literal, StringComparison.Ordinal))
                    throw new FormatException("Invalid JSON literal.");
                _index += literal.Length;
            }

            private void Expect(char expected)
            {
                SkipWhiteSpace();
                if (IsEnd || _json[_index] != expected) throw new FormatException("Expected '" + expected + "'.");
                _index++;
            }
        }
    }
}
