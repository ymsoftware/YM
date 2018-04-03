using System;
using System.Collections;
using System.Text;

namespace YM.Json
{
    public class JsonValue
    {
        private readonly object _value;

        public JsonType Type { get; private set; }

        #region Constructors
        
        public JsonValue(JsonObject value)
        {
            _value = value;
            Type = JsonType.Object;
        }

        public JsonValue(JsonArray value)
        {
            _value = value;
            Type = JsonType.Array;
        }

        public JsonValue(string value)
        {
            _value = value;
            Type = JsonType.String;
        }

        public JsonValue(int value)
        {
            _value = value;
            Type = JsonType.Number;
        }

        public JsonValue(long value)
        {
            _value = value;
            Type = JsonType.Number;
        }

        public JsonValue(short value)
        {
            _value = value;
            Type = JsonType.Number;
        }

        public JsonValue(float value)
        {
            _value = value;
            Type = JsonType.Number;
        }

        public JsonValue(double value)
        {
            _value = value;
            Type = JsonType.Number;
        }

        public JsonValue(decimal value)
        {
            _value = value;
            Type = JsonType.Number;
        }

        public JsonValue(bool value)
        {
            _value = value;
            Type = JsonType.Boolean;
        }

        public JsonValue(object value)
        {
            if (value == null)
            {
                Type = JsonType.Null;
            }
            else if (value is string)
            {
                Type = JsonType.String;
            }
            else if (value is JsonObject)
            {
                Type = JsonType.Object;
            }
            else if (value is JsonArray)
            {
                Type = JsonType.Array;
            }
            else if (value is int || value is float || value is double || value is long || value is short || value is byte || value is decimal)
            {
                Type = JsonType.Number;
            }
            else if (value is bool)
            {
                Type = JsonType.Boolean;
            }
            else if (value is IEnumerable)
            {
                _value = new JsonArray(value as IEnumerable);
                Type = JsonType.Array;
                return;
            }
            else if (value is JsonValue)
            {
                var jv = value as JsonValue;
                _value = jv.Get();
                Type = jv.Type;
                return;
            }
            else
            {
                _value = value.ToString();
                Type = JsonType.String;
                return;
            }

            _value = value;
        }

        #endregion

        public object Get()
        {
            return _value;
        }

        public T Get<T>()
        {
            try
            {
                return (T)_value;
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException && Type == JsonType.Number)
                {
                    if (_value is int)
                    {
                        if (typeof(T) == typeof(long))
                        {
                            return (T)(object)Convert.ToInt64(_value);
                        }
                        if (typeof(T) == typeof(short))
                        {
                            return (T)(object)Convert.ToInt16(_value);
                        }
                        if (typeof(T) == typeof(byte))
                        {
                            return (T)(object)Convert.ToByte(_value);
                        }
                    }
                    if (_value is double)
                    {
                        if (typeof(T) == typeof(float))
                        {
                            return (T)(object)Convert.ToSingle(_value);
                        }
                        if (typeof(T) == typeof(decimal))
                        {
                            return (T)(object)Convert.ToDecimal(_value);
                        }
                    }
                }

                return default(T);
            }
        }

        public override string ToString()
        {
            return ToString(true, 0);
        }

        #region Implicit operators

        public static implicit operator JsonValue(string value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(JsonObject value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(JsonArray value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(int value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(long value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(short value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(float value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(double value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(decimal value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(bool value)
        {
            return new JsonValue(value);
        }

        #endregion

        internal string ToString(bool pretty, int level)
        {
            if (Type == JsonType.String)
            {
                return AsPhrase((string)_value, false);
            }

            if (Type == JsonType.Boolean)
            {
                return (bool)_value ? "true" : "false";
            }

            if (Type == JsonType.Object)
            {
                return ((JsonObject)_value).ToString(pretty, level);
            }

            if (Type == JsonType.Array)
            {
                return ((JsonArray)_value).ToString(pretty, level);
            }

            if (Type == JsonType.Null)
            {
                return "null";
            }

            return _value.ToString();
        }

        private string AsPhrase(string s, bool ignorePhrase = true)
        {
            if (s == null)
            {
                return null;
            }

            if (ignorePhrase && IsPhrase(s))
            {
                return s;
            }

            var sb = new StringBuilder(s.Length + 10);
            sb.Append('"');

            foreach (char c in s)
            {
                if (c == '\"')
                {
                    sb.Append("\\\"");
                }
                else if (c == CharJsonParser.TOKEN_BACKSLASH)
                {
                    sb.Append(@"\\");
                }
                else if (c == CharJsonParser.TOKEN_CR)
                {
                    sb.Append("\\r");
                }
                else if (c == CharJsonParser.TOKEN_LF)
                {
                    sb.Append("\\n");
                }
                else if (c == CharJsonParser.TOKEN_BS)
                {
                    sb.Append("\\b");
                }
                else if (c == CharJsonParser.TOKEN_FF)
                {
                    sb.Append("\\f");
                }
                else if (c == CharJsonParser.TOKEN_HT)
                {
                    sb.Append("\\t");
                }
                else if (c == CharJsonParser.TOKEN_VT)
                {
                    sb.Append(" ");
                }
                else
                {
                    sb.Append(c);
                }
            }

            sb.Append('"');

            return sb.ToString();
        }

        private bool IsPhrase(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            int size = s.Length;
            if (size == 1)
            {
                return false;
            }

            if (s[0] == '"' && s[size - 1] == '"')
            {
                int i = 1;
                while (i < size - 1)
                {
                    if (s[i] == '"' && s[i - 1] != '\\')
                    {
                        return false;
                    }
                    i++;
                }

                return true;
            }

            return false;
        }
    }
}
