using System.Collections.Generic;
using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Query.CompoundQueries
{
    public class BoolQuery : QueryBase
    {
        private readonly IDictionary<string, IQuery[]> _queries = new Dictionary<string, IQuery[]>();

        public const string MUST = "must";
        public const string MUST_NOT = "must_not";
        public const string SHOULD = "should";
        public const string FILTER = "filter";

        public BoolQuery() { }

        public BoolQuery(IQuery[] must, IQuery[] not, IQuery[] should, IQuery[] filter)
        {
            _queries.Add(MUST, must);
            _queries.Add(MUST_NOT, not);
            _queries.Add(SHOULD, should);
            _queries.Add(FILTER, filter);
        }

        public BoolQuery Must(IQuery must)
        {
            Add(new IQuery[] { must }, MUST);
            return this;
        }

        public BoolQuery Must(IQuery[] must)
        {
            Add(must, MUST);
            return this;
        }

        public BoolQuery Should(IQuery should)
        {
            Add(new IQuery[] { should }, SHOULD);
            return this;
        }

        public BoolQuery Should(IQuery[] should)
        {
            Add(should, SHOULD);
            return this;
        }

        public BoolQuery Filter(IQuery filter)
        {
            Add(new IQuery[] { filter }, FILTER);
            return this;
        }

        public BoolQuery Filter(IQuery[] filter)
        {
            Add(filter, FILTER);
            return this;
        }

        public BoolQuery Not(IQuery not)
        {
            Add(new IQuery[] { not }, MUST_NOT);
            return this;
        }

        public BoolQuery Not(IQuery[] not)
        {
            Add(not, MUST_NOT);
            return this;
        }

        public IQuery[] MustQueries
        {
            get
            {
                return GetQueries(MUST);
            }
        }

        public IQuery[] NotQueries
        {
            get
            {
                return GetQueries(MUST_NOT);
            }
        }

        public IQuery[] ShouldQueries
        {
            get
            {
                return GetQueries(SHOULD);
            }
        }

        public IQuery[] FilterQueries
        {
            get
            {
                return GetQueries(FILTER);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _queries.Count == 0;
            }
        }

        public override QueryType Type => QueryType.BoolQuery;

        public override JsonObject ToJson()
        {
            var @bool = new JsonObject();
            var plus = new List<IQuery>();
            var minus = new List<IQuery>();

            AddJson(@bool, MUST, plus);
            AddJson(@bool, MUST_NOT, minus);
            AddJson(@bool, SHOULD, plus);
            AddJson(@bool, FILTER, plus);

            if (minus.Count == 0)
            {
                if (plus.Count == 0)
                {
                    return null;
                }
                else if (plus.Count == 1)
                {
                    return plus[0].ToJson();
                }
            }

            return new JsonObject().Add("bool", @bool);
        }

        private IQuery[] GetQueries(string key)
        {
            if (_queries.TryGetValue(key, out IQuery[] queries))
            {
                return queries;
            }

            return null;
        }

        private void Add(IQuery[] queries, string key)
        {
            if (_queries.TryGetValue(key, out IQuery[] existing))
            {
                _queries[key] = existing.Union(queries).ToArray();
            }
            else
            {
                _queries.Add(key, queries);
            }
        }

        private void AddJson(JsonObject @bool, string key, List<IQuery> all)
        {
            var queries = GetQueries(key);

            if (queries == null || queries.Length == 0)
            {
                return;
            }

            if (queries.Length == 1)
            {
                @bool.Add(key, queries[0].ToJson());
                all.Add(queries[0]);
            }
            else
            {
                @bool.Add(key, new JsonArray(queries.Select(e => e.ToJson())));
                all.AddRange(queries);
            }
        }
    }
}