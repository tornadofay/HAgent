using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            return new ToolValidationResult(false, new Dictionary<string, object>(), errors.ToList().AsReadOnly());
        }
    }

    public static class ToolSchemaValidator
    {
        private static readonly Regex JsonPropertyName = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        public static ToolValidationResult Validate(AiTool definition, IDictionary<string, object> arguments)
        {
            if (definition == null) return ToolValidationResult.Failure(new[] { "Tool definition is required." });
            if (string.IsNullOrWhiteSpace(definition.InputSchemaJson)) return ToolValidationResult.Success(new Dictionary<string, object>(arguments ?? new Dictionary<string, object>()));

            JObject schema;
            try { schema = JObject.Parse(definition.InputSchemaJson); }
            catch (Exception ex) { return ToolValidationResult.Failure(new[] { "Tool input schema is invalid JSON: " + ex.Message }); }

            var source = new Dictionary<string, object>(arguments ?? new Dictionary<string, object>(), StringComparer.OrdinalIgnoreCase);
            var errors = new List<string>();
            ValidateObject(schema, source, "$", errors);
            if (errors.Count > 0) return ToolValidationResult.Failure(errors);

            var normalized = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in source) normalized[pair.Key] = pair.Value;
            return ToolValidationResult.Success(normalized);
        }

        private static void ValidateObject(JObject schema, IDictionary<string, object> args, string path, ICollection<string> errors)
        {
            var type = schema["type"] == null ? null : schema["type"].ToString();
            if (!string.IsNullOrWhiteSpace(type) && !string.Equals(type, "object", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(path + ": root tool arguments must use type 'object'.");
                return;
            }

            var properties = schema["properties"] as JObject;
            var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var requiredArray = schema["required"] as JArray;
            if (requiredArray != null)
                foreach (var token in requiredArray.OfType<JValue>())
                    if (token.Type == JTokenType.String) required.Add(token.ToString());

            foreach (var name in required)
                if (!args.ContainsKey(name) || args[name] == null)
                    errors.Add(path + "." + name + ": required argument is missing.");

            if (properties == null) return;

            var allowAdditional = schema["additionalProperties"] == null || schema["additionalProperties"].Type != JTokenType.Boolean || schema["additionalProperties"].Value<bool>();
            foreach (var name in args.Keys)
            {
                var property = properties.Properties().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                {
                    if (!allowAdditional) errors.Add(path + "." + name + ": argument is not allowed by the schema.");
                    continue;
                }
                ValidateValue(property.Value as JObject, args[name], path + "." + property.Name, errors);
            }
        }

        private static void ValidateValue(JObject schema, object value, string path, ICollection<string> errors)
        {
            if (schema == null || value == null) return;
            var expected = schema["type"] == null ? null : schema["type"].ToString();
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
                    if (!(value is System.Collections.IEnumerable) || value is string) errors.Add(path + ": expected an array.");
                    break;
                case "object":
                    if (!(value is IDictionary<string, object>) && !(value is JObject) && !(value is IDictionary<string, string>)) errors.Add(path + ": expected an object.");
                    break;
            }

            var enumValues = schema["enum"] as JArray;
            if (enumValues != null && enumValues.Count > 0)
            {
                var matched = enumValues.Any(x => string.Equals(Convert.ToString(x, CultureInfo.InvariantCulture), Convert.ToString(value, CultureInfo.InvariantCulture), StringComparison.Ordinal));
                if (!matched) errors.Add(path + ": value is not one of the permitted enum values.");
            }

            var pattern = schema["pattern"] == null ? null : schema["pattern"].ToString();
            if (!string.IsNullOrWhiteSpace(pattern) && value is string)
            {
                try
                {
                    if (!Regex.IsMatch((string)value, pattern)) errors.Add(path + ": string does not match the required pattern.");
                }
                catch (ArgumentException) { errors.Add(path + ": schema contains an invalid regex pattern."); }
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
    }
}
