using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YM.Json
{
    public static class TemplateExtensions
    {
        public static JsonObject SetParameters(this JsonObject jo, IDictionary<string, object> parameters)
        {
            if (jo == null || jo.IsEmpty)
            {
                return jo;
            }

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            var @new = new JsonObject();

            foreach (var jp in jo.Properties())
            {
                var value = SetParameters(jp.Name, parameters);
                if (value != null)
                {
                    string name = value as string;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        value = SetParameter(jp.Value, parameters);
                        if (value != null)
                        {
                            @new.Add(name, value);
                        }
                    }
                }
            }

            return @new;
        }

        private static object SetParameters(string s, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var chars = s.ToCharArray();
            int size = chars.Length;
            int i = 0;
            int raw = 0;
            StringBuilder sb = null;
            bool parameterized = false;

            while (i < size)
            {
                char c = chars[i];

                if (c == '$' && i < size - 3 && chars[++i] == '{')
                {
                    int start = i;
                    i++;
                    int end = 0;
                    int question = 0;

                    while (i < size)
                    {
                        c = chars[i];
                        if (c == '}')
                        {
                            end = i - 1;
                            break;
                        }
                        else if (c == '?')
                        {
                            question = i;
                        }
                        i++;
                    }

                    if (end > start)
                    {
                        int len = question > 0 ? question - 1 - start : end - start;
                        string key = s.Substring(start + 1, len);

                        if (parameters.TryGetValue(key, out object value))
                        {
                            //do nothing
                        }
                        else if (question > 0 && end > question)
                        {
                            value = s.Substring(question + 1, end - question);
                        }
                        else
                        {
                            return null;
                        }

                        if (start == 1 && end == size - 2)
                        {
                            return value;
                        }
                        else
                        {
                            if (sb == null)
                            {
                                sb = new StringBuilder();
                            }

                            sb
                                .Append(s.Substring(raw, start - raw - 1))
                                .Append(value.ToString());

                            raw = i + 1;
                        }

                        parameterized = true;
                    }
                }

                i++;
            }

            if (parameterized)
            {
                if (i > raw)
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }

                    sb.Append(s.Substring(raw, size - raw));                    
                }

                return sb == null || sb.Length == 0 ? null : sb.ToString();
            }
            
            return s;
        }

        private static object SetParameter(JsonValue jv, IDictionary<string, object> parameters)
        {
            switch (jv.Type)
            {
                case JsonType.String:
                    return SetParameters(jv.Get<string>(), parameters);
                case JsonType.Boolean:
                case JsonType.Number:
                    return jv.Get();
                case JsonType.Object:
                    var temp = (jv.Get<JsonObject>()).SetParameters(parameters);
                    return temp == null || temp.IsEmpty ? null : temp;
                case JsonType.Array:
                    var ja = (jv.Get<JsonArray>())
                        .Select(e => SetParameter(e, parameters))
                        .Where(e => e != null)
                        .ToArray();
                    return ja.Length == 0 ? null : new JsonArray(ja);
                default:
                    return null;
            }
        }
    }
}