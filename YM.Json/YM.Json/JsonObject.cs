using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YM.Json
{
    public class JsonObject
    {
        private readonly IDictionary<string, JsonProperty> _properties = new Dictionary<string, JsonProperty>();

        public JsonObject() { }

        public JsonObject(IDictionary<string, JsonProperty> properties)
        {
            if (properties != null)
            {
                _properties = properties;
            }           
        }

        public JsonObject(IDictionary<string, object> properties)
        {
            if (properties != null)
            {
                _properties = properties.ToDictionary(e => e.Key, e => new JsonProperty(e.Key, e.Value));
            }
        }

        public static JsonObject Parse(string json)
        {
            var jv = json.ToJson();

            if (jv.Type == JsonType.Object)
            {
                return jv.Get<JsonObject>();
            }

            throw new JsonParseException("Expected JsonObject, found '{0}'", jv.Type);
        }

        public JsonObject Add(JsonProperty property)
        {
            _properties.Add(property.Name, property);
            return this;
        }

        public JsonObject Add(string name, JsonValue value)
        {
            _properties.Add(name, new JsonProperty(name, value));
            return this;
        }

        public JsonObject Add(string name, object value)
        {
            _properties.Add(name, new JsonProperty(name, value));
            return this;
        }

        public JsonObject Remove(string name)
        {
            _properties.Remove(name);
            return this;
        }

        public JsonProperty Property(string name)
        {

            if (_properties.TryGetValue(name, out JsonProperty property))
            {
                return property;
            }

            return null;
        }

        public JsonProperty[] Properties()
        {
            return _properties.Values.ToArray();
        }

        public T Property<T>(string name)
        {
            if (_properties.TryGetValue(name, out JsonProperty property))
            {
                var value = property.Value;
                if (value != null) return value.Get<T>();
            }

            return default(T);
        }

        public bool IsEmpty
        {
            get { return _properties.Count == 0; }
        }

        public JsonObject Copy()
        {
            if (_properties.Count == 0)
            {
                return new JsonObject();
            }

            var copy = new KeyValuePair<string, JsonProperty>[_properties.Count];
            _properties.CopyTo(copy, 0);
            var properties = copy.ToDictionary(e => e.Key, e => e.Value);
            return new JsonObject(properties);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return _properties.ToDictionary(e => e.Key, e => e.Value.Value.Get());
        }

        public T PathValue<T>(string path)
        {
            var jp = new JsonPath(path);
            return jp.GetValue<T>(this);
        }

        public T[] PathValues<T>(string path)
        {
            var jp = new JsonPath(path);
            return jp.GetValues<T>(this);
        }

        public string ToString(bool pretty)
        {
            return ToString(pretty, 0);
        }

        public override string ToString()
        {
            return ToString(true);
        }

        internal string ToString(bool pretty, int level)
        {
            int next = level + 1;

            var values = _properties.Select(e =>
            {
                string key = string.Format("\"{0}\"", e.Key);
                string value = e.Value.Value.ToString(pretty, next);
                return string.Format("{0}{1}{2}", key, pretty ? ": " : ":", value);
            });

            if (pretty)
            {
                string tab = "  ";

                var sb = new StringBuilder();
                for (int i = 0; i < level; i++)
                {
                    sb.Append("  ");
                }
                string tabs = sb.ToString();
                string lb = string.Format(",\n{0}  ", tabs);

                return string.Concat("{\r\n", tabs, tab, string.Join(lb, values), "\r\n", tabs, "}");
            }

            return string.Concat("{", string.Join(",", values), "}");
        }        
    }
}
