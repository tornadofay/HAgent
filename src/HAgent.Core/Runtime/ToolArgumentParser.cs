using System;
using System.Collections.Generic;
using System.Globalization;

namespace HAgent.Runtime
{
    public static class ToolArgumentParser
    {
        public static bool TryParseObject(string json, out IDictionary<string, object> arguments, out string error)
        {
            arguments = null;
            error = string.Empty;
            try
            {
                var parser = new Parser(json ?? string.Empty);
                var value = parser.ParseValue();
                parser.SkipWhiteSpace();
                if (!parser.IsEnd) throw new FormatException("Unexpected characters after the JSON value.");
                arguments = value as Dictionary<string, object>;
                if (arguments == null) throw new FormatException("Tool arguments must be a JSON object.");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private sealed class Parser
        {
            private readonly string _json;
            private int _index;
            public Parser(string json) { _json = json; }
            public bool IsEnd { get { return _index >= _json.Length; } }

            public void SkipWhiteSpace()
            {
                while (!IsEnd && char.IsWhiteSpace(_json[_index])) _index++;
            }

            public object ParseValue()
            {
                SkipWhiteSpace();
                if (IsEnd) throw new FormatException("Unexpected end of JSON.");
                switch (_json[_index])
                {
                    case '{': return ParseObject();
                    case '[': return ParseArray();
                    case '"': return ParseString();
                    case 't': Literal("true"); return true;
                    case 'f': Literal("false"); return false;
                    case 'n': Literal("null"); return null;
                    default: return ParseNumber();
                }
            }

            private Dictionary<string, object> ParseObject()
            {
                var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                Expect('{');
                SkipWhiteSpace();
                if (!IsEnd && _json[_index] == '}') { _index++; return result; }
                while (true)
                {
                    SkipWhiteSpace();
                    var name = ParseString();
                    Expect(':');
                    result[name] = ParseValue();
                    SkipWhiteSpace();
                    if (!IsEnd && _json[_index] == '}') { _index++; return result; }
                    Expect(',');
                }
            }

            private List<object> ParseArray()
            {
                var result = new List<object>();
                Expect('[');
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
                var hasDecimalOrExponent = false;

                while (!IsEnd)
                {
                    var c = _json[_index];
                    if ("0123456789-+".IndexOf(c) >= 0)
                    {
                        _index++;
                        continue;
                    }

                    if (c == '.' || c == 'e' || c == 'E')
                    {
                        hasDecimalOrExponent = true;
                        _index++;
                        continue;
                    }

                    break;
                }

                var token = _json.Substring(start, _index - start);
                if (string.IsNullOrWhiteSpace(token))
                    throw new FormatException("Invalid JSON number.");

                if (!hasDecimalOrExponent)
                {
                    long integerValue;
                    if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out integerValue))
                    {
                        if (integerValue >= int.MinValue && integerValue <= int.MaxValue)
                            return (int)integerValue;
                        return integerValue;
                    }
                }

                decimal decimalValue;
                if (decimal.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out decimalValue))
                    return decimalValue;

                throw new FormatException("Invalid JSON number.");
            }

            private void Literal(string value)
            {
                if (_index + value.Length > _json.Length || !string.Equals(_json.Substring(_index, value.Length), value, StringComparison.Ordinal))
                    throw new FormatException("Invalid JSON literal.");
                _index += value.Length;
            }

            private void Expect(char value)
            {
                SkipWhiteSpace();
                if (IsEnd || _json[_index] != value) throw new FormatException("Expected '" + value + "'.");
                _index++;
            }
        }
    }
}
