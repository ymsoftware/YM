using System.Linq;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Elasticsearch.Query.SpecializedQueries;
using YM.Elasticsearch.Query.TermQueries;
using YM.Json;

namespace YM.Elasticsearch.Query
{
    public static class QueryExtensions
    {
        public static IQuery ToQuery(this JsonObject jo)
        {
            if (jo == null || jo.IsEmpty)
            {
                return null;
            }

            var jp = jo.Properties()[0];
            var query = jp.Value.Get<JsonObject>();

            if (query == null)
            {
                return null;
            }

            switch (jp.Name)
            {
                case "term": return ParseTermQuery(query);
                case "terms": return ParseTermsQuery(query);
                case "prefix": return ParsePrefixQuery(query);
                case "wildcard": return ParseWildcardQuery(query);
                case "range": return ParseRangeQuery(query);
                case "exists": return ParseExistsQuery(query);
                case "match": return ParseMatchQuery(query);
                case "match_phrase": return ParseMatchPhraseQuery(query);
                case "match_phrase_prefix": return ParseMatchPhrasePrefixQuery(query);
                case "query_string": return ParseQueryStringQuery(query);
                case "simple_query_string": return ParseSimpleQueryStringQuery(query);
                case "multi_match": return ParseMultiMatchQuery(query);
                case "bool": return ParseBoolQuery(query);
                case "constant_score": return ParseConstantScoreQuery(query);
                case "function_score": return ParseFunctionScoreQuery(query);
                case "percolate": return ParsePercolateQuery(query);
                case "match_all": return new MatchAllQuery();
            }

            return null;
        }

        private static TermQuery ParseTermQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var value = jp.Value.Get();
            if (value == null)
            {
                return null;
            }

            return new TermQuery(jp.Name, value);
        }

        private static TermsQuery ParseTermsQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var values = jp.Value.Get<JsonArray>();
            if (values == null || values.Length == 0)
            {
                return null;
            }

            return new TermsQuery(jp.Name, values.Select(e => e.Get()).ToArray());
        }

        private static PrefixQuery ParsePrefixQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var value = jp.Value.Get<string>();
            if (value == null)
            {
                return null;
            }

            return new PrefixQuery(jp.Name, value);
        }

        private static WildcardQuery ParseWildcardQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var value = jp.Value.Get<string>();
            if (value == null)
            {
                return null;
            }

            return new WildcardQuery(jp.Name, value);
        }

        private static RangeQuery ParseRangeQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var range = jp.Value.Get<JsonObject>();
            if (range == null || range.IsEmpty)
            {
                return null;
            }

            RangeValue from = null;
            RangeValue to = null;

            foreach (var p in range.Properties())
            {
                var value = p.Value.Get();

                switch (p.Name)
                {
                    case "gt": from = new RangeValue(value, false); break;
                    case "gte": from = new RangeValue(value, true); break;
                    case "lt": to = new RangeValue(value, false); break;
                    case "lte": to = new RangeValue(value, true); break;
                }
            }

            if (from == null && to == null)
            {
                return null;
            }

            return new RangeQuery(jp.Name, new RangeValues(from, to));
        }

        private static ExistsQuery ParseExistsQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            string field = jp.Value.Get<string>();
            if (string.IsNullOrWhiteSpace(field) || jp.Name != "field")
            {
                return null;
            }

            return new ExistsQuery(field);
        }

        private static MatchQuery ParseMatchQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var value = jp.Value.Get();
            if (value == null)
            {
                return null;
            }

            switch (jp.Value.Type)
            {
                case JsonType.String:
                    return new MatchQuery(jp.Name, (string)value);
                case JsonType.Object:
                    var match = (JsonObject)value;
                    string query = match.Property<string>("query");
                    if (string.IsNullOrWhiteSpace(query))
                    {
                        return null;
                    }

                    bool isAnd = match.Property<string>("operator") == "and";
                    bool isZeroTerms = match.Property<string>("zero_terms_query") == "all";

                    return new MatchQuery(jp.Name, query, isAnd, isZeroTerms);
                default:
                    return null;
            }
        }

        private static MatchPhraseQuery ParseMatchPhraseQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            string value = jp.Value.Get<string>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return new MatchPhraseQuery(jp.Name, value);
        }

        private static MatchPhrasePrefixQuery ParseMatchPhrasePrefixQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            string value = jp.Value.Get<string>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return new MatchPhrasePrefixQuery(jp.Name, value);
        }

        private static QueryStringQuery ParseQueryStringQuery(JsonObject jo)
        {
            string query = null;
            string[] fields = null;
            bool isAnd = false;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "query": query = jp.Value.Get<string>(); break;
                    case "fields": fields = jp.Value.Get<JsonArray>().Select(e => e.Get<string>()).ToArray(); break;
                    case "default_operator": isAnd = jp.Value.Get<string>() == "and"; break;
                }
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            return new QueryStringQuery(query, fields, isAnd);
        }

        private static SimpleQueryStringQuery ParseSimpleQueryStringQuery(JsonObject jo)
        {
            string query = null;
            string[] fields = null;
            bool isAnd = false;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "query": query = jp.Value.Get<string>(); break;
                    case "fields": fields = jp.Value.Get<JsonArray>().Select(e => e.Get<string>()).ToArray(); break;
                    case "default_operator": isAnd = jp.Value.Get<string>() == "and"; break;
                }
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            return new SimpleQueryStringQuery(query, fields, isAnd);
        }

        private static MultiMatchQuery ParseMultiMatchQuery(JsonObject jo)
        {
            string query = null;
            string[] fields = null;
            MultiMatchType type = MultiMatchType.BestFields;
            bool isAnd = false;
            double tieBreaker = 0;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "query": query = jp.Value.Get<string>(); break;
                    case "fields": fields = jp.Value.Get<JsonArray>().Select(e => e.Get<string>()).ToArray(); break;
                    case "operator": isAnd = jp.Value.Get<string>() == "and"; break;
                    case "tie_breaker": tieBreaker = jp.Value.Get<double>(); break;
                    case "type": type = GetMultiMatchType(jp.Value.Get<string>()); break;
                }
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            return new MultiMatchQuery(query, fields, type, isAnd, tieBreaker);
        }

        private static BoolQuery ParseBoolQuery(JsonObject jo)
        {
            var @bool = new BoolQuery();

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "must": @bool.Must(GetBoolQueries(jp.Value)); break;
                    case "filter": @bool.Filter(GetBoolQueries(jp.Value)); break;
                    case "must_not": @bool.Not(GetBoolQueries(jp.Value)); break;
                    case "should": @bool.Should(GetBoolQueries(jp.Value)); break;
                }
            }

            if (@bool.IsEmpty)
            {
                return null;
            }

            return @bool;
        }

        private static ConstantScoreQuery ParseConstantScoreQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var query = jp.Value.Get<JsonObject>();
            if (query == null || (jp.Name != "filter" && jp.Name != "query"))
            {
                return null;
            }

            return new ConstantScoreQuery(query.ToQuery());
        }

        private static FunctionScoreQuery ParseFunctionScoreQuery(JsonObject jo)
        {
            IScoreFunction[] functions = null;
            IQuery query = null;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "query": query = jp.Value.Get<JsonObject>().ToQuery(); break;
                    case "functions": functions = jp.Value.Get<JsonArray>().Select(e => e.Get<JsonObject>().ToFunction()).Where(e => e != null).ToArray(); break;
                }
            }

            if (functions == null || functions.Length == 0)
            {
                return null;
            }

            return new FunctionScoreQuery(query, functions);
        }

        private static PercolateQuery ParsePercolateQuery(JsonObject jo)
        {
            string field = null;
            JsonObject document = null;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "field": field = jp.Value.Get<string>(); break;
                    case "document": document = jp.Value.Get<JsonObject>(); break;
                }
            }

            if (string.IsNullOrWhiteSpace(field) || document == null || document.IsEmpty)
            {
                return null;
            }

            return new PercolateQuery(field, document);
        }

        private static IQuery[] GetBoolQueries(JsonValue jv)
        {
            if (jv.Type == JsonType.Object)
            {
                return new IQuery[] { jv.Get<JsonObject>().ToQuery() };
            }
            else if (jv.Type == JsonType.Array)
            {
                return jv.Get<JsonArray>()
                    .Select(e => e.Get<JsonObject>().ToQuery())
                    .ToArray();
            }

            return null;
        }

        private static MultiMatchType GetMultiMatchType(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return MultiMatchType.BestFields;
            }

            switch (s)
            {
                case "cross_fields": return MultiMatchType.CrossFields;
                case "most_fields": return MultiMatchType.MostFields;
                case "phrase": return MultiMatchType.Phrase;
                case "phrase_prefix": return MultiMatchType.PhrasePrefix;
            }

            return MultiMatchType.BestFields;
        }
    }
}