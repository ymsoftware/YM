using System.Collections.Generic;
using System.Linq;

namespace YM.Json
{
    class JsonPath
    {
        private readonly JsonPathToken[] _tokens;

        public JsonPath(string path)
        {
            _tokens = path
                .TrimStart(new char[] { '$', '@', '.' }) // no support yet
                .Split('.')
                .Select(e =>
                {
                    if (e.EndsWith("]"))
                    {
                        var tokens = e.TrimEnd(']').Split('[');
                        string index = tokens[1];
                        if (index == "*" || index == "")
                        {
                            return new JsonPathToken(tokens[0], true);
                        }
                        else
                        {
                            return new JsonPathToken(tokens[0], true, int.Parse(index));
                        }
                    }
                    else
                    {
                        return new JsonPathToken(e, false);
                    }
                })
                .ToArray();
        }

        public T GetValue<T>(JsonObject jo)
        {
            var last = GetLast(jo);
            if (last == null)
            {
                return default(T);
            }

            var token = _tokens[_tokens.Length - 1];
            var next = GetNext<T>(last, token);
            return next is IEnumerable<T> ? ((IEnumerable<T>)next).First() : (T)next;
        }

        public T[] GetValues<T>(JsonObject jo)
        {
            var last = GetLast(jo);
            if (last == null)
            {
                return null;
            }

            var token = _tokens[_tokens.Length - 1];
            var test = GetNext<T>(last, token);
            return test == null ? null : ((IEnumerable<T>)test).ToArray();
        }

        private object GetLast(JsonObject jo)
        {
            object last = jo;
            int index = 0;
            int size = _tokens.Length - 1;

            while (index < size && last != null)
            {
                var token = _tokens[index];
                last = GetNext<JsonObject>(last, token);
                index++;

            }

            return last;
        }

        private object GetNext<T>(object parent, JsonPathToken token)
        {
            if (token.IsArray)
            {
                if (parent is IEnumerable<JsonObject>)
                {
                    var list = new List<T>();

                    foreach (var item in (parent as IEnumerable<JsonObject>))
                    {
                        var ja = item.Property<JsonArray>(token.Name);
                        if (ja != null)
                        {
                            if (token.Index == -1)
                            {
                                var add = ja.Select(e => e.Get<T>());
                                list.AddRange(add);
                            }
                            else
                            {
                                var add= ja.Get<T>(token.Index);
                                if (add != null) list.Add(add);
                            }
                        }
                    }

                    return list;
                }
                else
                {
                    if (token.Index == -1)
                    {
                        var test = (parent as JsonObject).Property<JsonArray>(token.Name);
                        return test?.Select(e => e.Get<T>());
                    }
                    else
                    {
                        var test = (parent as JsonObject).Property<JsonArray>(token.Name);
                        if (test == null)
                        {
                            return null;
                        }
                        else
                        {
                            return test.Get<T>(token.Index);
                        }
                    }
                }
            }
            else
            {
                if (parent is IEnumerable<JsonObject>)
                {
                    return (parent as IEnumerable<JsonObject>).Select(e => (e as JsonObject).Property<T>(token.Name));
                }
                else
                {
                    return (parent as JsonObject).Property<T>(token.Name);
                }
            }
        }
    }
}
