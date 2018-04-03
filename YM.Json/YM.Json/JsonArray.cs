using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YM.Json
{
    public class JsonArray : IEnumerable<JsonValue>
    {
        private readonly List<JsonValue> _items = new List<JsonValue>();

        public JsonArray() { }

        public JsonArray(IEnumerable items)
        {
            if (items != null)
            {
                var enumerator = items.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    object item = enumerator.Current;
                    Add(item);
                }
            }
        }

        public static JsonArray Parse(string json)
        {
            var jv = json.ToJson();

            if (jv.Type == JsonType.Array)
            {
                return jv.Get<JsonArray>();
            }

            throw new JsonParseException("Expected JsonArray, found '{0}'", jv.Type);
        }

        public JsonArray Add(JsonValue item)
        {
            _items.Add(item);
            return this;
        }

        public JsonArray Add(object item)
        {
            return Add(new JsonValue(item));
        }

        public T Get<T>(int index)
        {
            int count = _items.Count;
            if (count > 0 && index >= 0 && index < count)
            {
                return _items[index].Get<T>();
            }

            return default(T);
        }

        public int Length
        {
            get
            {
                return _items.Count;
            }
        }

        public JsonArray Copy()
        {
            var ja = new JsonArray();

            foreach (var value in _items)
            {
                switch (value.Type)
                {
                    case JsonType.Object: ja.Add((value.Get<JsonObject>()).Copy()); break;
                    case JsonType.Array: ja.Add((value.Get<JsonArray>()).Copy()); break;
                    default: ja.Add(value.Get()); break;
                }
            }

            return ja;
        }

        public string ToString(bool pretty = false)
        {
            return ToString(pretty, 0);
        }

        public override string ToString()
        {
            return ToString(true);
        }

        internal string ToString(bool pretty, int level)
        {
            var values = _items.Select(e => e.ToString(pretty, level + 1));

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

                return string.Concat("[\n", tabs, tab, string.Join(lb, values), "\n", tabs, "]");
            }

            return string.Concat("[", string.Join(",", values), "]");
        }

        public IEnumerator<JsonValue> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
