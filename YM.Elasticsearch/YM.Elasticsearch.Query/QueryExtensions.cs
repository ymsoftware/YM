using System.Collections.Generic;
using System.Linq;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Elasticsearch.Query.FullTextQueries.QueryString;
using YM.Elasticsearch.Query.JoiningQueries;
using YM.Elasticsearch.Query.SpanQueries;
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
                case "nested": return ParseNestedQuery(query);                
                case "span_term": return ParseSpanTermQuery(query);
                case "span_near": return ParseSpanNearQuery(query);
                case "span_multi": return ParseSpanMultiTermQuery(query);                
                case "match_all": return new MatchAllQuery();

                //1.x leftovers
                case "and": return new BoolQuery().Must(GetBoolQueries(query));
                case "or": return new BoolQuery().Should(GetBoolQueries(query));
                case "not": return new BoolQuery().Not(GetBoolQueries(query));
            }

            return null;
        }

        public static T[] GetFilters<T>(this IQuery query, string field, T[] defaults)
        {
            IQuery q = null;

            if (query is ConstantScoreQuery)
            {
                q = (query as ConstantScoreQuery).Query;
            }
            else if (query is FunctionScoreQuery)
            {
                q = (query as FunctionScoreQuery).Query;
            }
            else
            {
                q = query;
            }

            if (q is TermQuery)
            {
                var tq = q as TermQuery;
                if (tq.Field == field)
                {
                    return new T[] { (T)tq.Value };
                }
            }
            else if (q is TermsQuery)
            {
                var tq = q as TermsQuery;
                if (tq.Field == field)
                {
                    return tq.Values.Select(e => (T)e).ToArray();
                }
            }
            else if (q is BoolQuery)
            {
                var bq = q as BoolQuery;

                var qs = new List<IQuery>();
                if (bq.MustQueries != null && bq.MustQueries.Length > 0)
                {
                    qs.AddRange(bq.MustQueries);
                }
                if (bq.FilterQueries != null && bq.FilterQueries.Length > 0)
                {
                    qs.AddRange(bq.FilterQueries);
                }

                if (qs.Count > 0)
                {
                    var values = new List<T>();
                    foreach (var cq in qs)
                    {
                        values.AddRange(cq.GetFilters<T>(field, new T[] { }));
                    }

                    if (values.Count > 0)
                    {
                        return values.Distinct().ToArray();
                    }
                }
            }

            return defaults;
        }

        public static T[] GetNotFilters<T>(this IQuery query, string field, T[] defaults)
        {
            IQuery q = null;

            if (query is ConstantScoreQuery)
            {
                q = (query as ConstantScoreQuery).Query;
            }
            else if (query is FunctionScoreQuery)
            {
                q = (query as FunctionScoreQuery).Query;
            }
            else
            {
                q = query;
            }

            if (q is BoolQuery)
            {
                var bq = q as BoolQuery;

                if (bq.NotQueries != null && bq.NotQueries.Length > 0)
                {
                    var values = new List<T>();
                    foreach (var nq in bq.NotQueries)
                    {
                        if (nq is TermQuery || nq is TermsQuery)
                        {
                            values.AddRange(nq.GetFilters<T>(field, new T[] { }));
                        }
                        else if (nq is BoolQuery)
                        {
                            values.AddRange(nq.GetNotFilters<T>(field, new T[] { }));
                        }
                    }

                    if (values.Count > 0)
                    {
                        return values.Distinct().ToArray();
                    }
                }
            }

            return defaults;
        }

        public static IQuery RemoveFilters(this IQuery query, string field, object[] values = null)
        {
            if (query is ConstantScoreQuery)
            {
                return new ConstantScoreQuery((query as ConstantScoreQuery).Query.RemoveFilters(field, values));
            }

            if (query is FunctionScoreQuery)
            {
                var fq = query as FunctionScoreQuery;
                return new FunctionScoreQuery(fq.Query.RemoveFilters(field, values), fq.Functions);
            }


            if (query is TermQuery)
            {
                var tq = query as TermQuery;
                if (tq.Field == field && (values == null || values.Contains(tq.Value)))
                {
                    return new MatchAllQuery();
                }
            }

            if (query is TermsQuery)
            {
                var tq = query as TermsQuery;
                if (tq.Field == field)
                {
                    if (values == null)
                    {
                        return new MatchAllQuery();
                    }
                    else
                    {
                        var except = tq.Values.Except(values).ToArray();
                        if (except.Length == 0)
                        {
                            return new MatchAllQuery();
                        }
                        return new TermsQuery(tq.Field, except);
                    }
                }
            }

            if (query is BoolQuery)
            {
                var bq = query as BoolQuery;

                var must = bq.MustQueries;
                if (must != null && must.Length > 0)
                {
                    must = must.Select(e => e.RemoveFilters(field, values)).Where(e => !(e is MatchAllQuery)).ToArray();
                    if (must.Length == 0) must = null;
                }

                var filter = bq.FilterQueries;
                if (filter != null && filter.Length > 0)
                {
                    filter = filter.Select(e => e.RemoveFilters(field, values)).Where(e => !(e is MatchAllQuery)).ToArray();
                    if (filter.Length == 0) filter = null;
                }

                if (must == null && filter == null && bq.ShouldQueries == null && bq.NotQueries == null)
                {
                    return new MatchAllQuery();
                }

                return new BoolQuery(must, bq.NotQueries, bq.ShouldQueries, filter);
            }

            return query;
        }

        public static IQuery RemoveNotFilters(this IQuery query, string field, object[] values = null)
        {
            if (query is ConstantScoreQuery)
            {
                return new ConstantScoreQuery((query as ConstantScoreQuery).Query.RemoveNotFilters(field, values));
            }

            if (query is FunctionScoreQuery)
            {
                var fq = query as FunctionScoreQuery;
                return new FunctionScoreQuery(fq.Query.RemoveNotFilters(field, values), fq.Functions);
            }

            if (query is BoolQuery)
            {
                var bq = query as BoolQuery;

                if (bq.NotQueries != null && bq.NotQueries.Length > 0)
                {
                    for (int i = 0; i < bq.NotQueries.Length; i++)
                    {
                        var nq = bq.NotQueries[i];

                        if (nq is TermQuery)
                        {
                            var tq = nq as TermQuery;
                            if (tq.Field == field && (values == null || values.Contains(tq.Value)))
                            {
                                bq.NotQueries[i] = null;
                            }
                        }
                        else if (nq is TermsQuery)
                        {
                            var tq = nq as TermsQuery;
                            if (tq.Field == field)
                            {
                                if (values == null)
                                {
                                    bq.NotQueries[i] = null;
                                }
                                else
                                {
                                    var except = tq.Values.Except(values).ToArray();
                                    if (except.Length == 0)
                                    {
                                        bq.NotQueries[i] = new MatchAllQuery();
                                    }
                                    else
                                    {
                                        bq.NotQueries[i] = new TermsQuery(tq.Field, except);
                                    }
                                }
                            }
                        }
                        else if (nq is BoolQuery)
                        {
                            bq.NotQueries[i] = RemoveNotFilters(nq, field, values);
                        }
                    }

                    var mustnot = bq.NotQueries.Where(e => e != null && !(e is MatchAllQuery)).ToArray();

                    if (mustnot.Length == 0) mustnot = null;

                    if (mustnot == null && bq.MustQueries == null && bq.ShouldQueries == null && bq.FilterQueries == null)
                    {
                        return new MatchAllQuery();
                    }

                    return new BoolQuery(bq.MustQueries, mustnot, bq.ShouldQueries, bq.FilterQueries);
                }
            }

            return query;
        }

        public static QueryString ToQueryString(this string s, bool fixQuery = true)
        {
            return new QueryStringParser().Parse(s);
        }

        #region Query parsing

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

        private static NestedQuery ParseNestedQuery(JsonObject jo)
        {
            string path = null;
            IQuery query = null;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "path": path = jp.Value.Get<string>(); break;
                    case "query": query = jp.Value.Get<JsonObject>().ToQuery(); break;
                }
            }

            if (string.IsNullOrWhiteSpace(path) || query == null)
            {
                return null;
            }

            return new NestedQuery(path, query);
        }

        private static SpanTermQuery ParseSpanTermQuery(JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var value = jp.Value.Get();
            if (value == null)
            {
                return null;
            }

            return new SpanTermQuery(jp.Name, value);
        }

        private static SpanNearQuery ParseSpanNearQuery(JsonObject jo)
        {
            IQuery[] clauses = null;
            bool ordered = false;
            int? slop = null;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "clauses": clauses = jp.Value.Get<JsonArray>().Select(e => e.Get<JsonObject>().ToQuery()).ToArray(); break;
                    case "in_order": ordered = jp.Value.Get<bool>(); break;
                    case "slop": slop = jp.Value.Get<int>(); break;
                }
            }

            if (clauses == null || clauses.Length == 0)
            {
                return null;
            }

            return new SpanNearQuery(clauses, ordered, slop);
        }

        private static SpanMultiTermQuery ParseSpanMultiTermQuery(JsonObject jo)
        {
            var match = jo.Property<JsonObject>("match");
            if (match == null)
            {
                return null;
            }

            var query = match.ToQuery();
            if (query == null)
            {
                return null;
            }

            return new SpanMultiTermQuery(query);
        }

        private static BoolQuery ParseNotQuery(JsonObject jo)
        {
            return new BoolQuery().Not(GetBoolQueries(jo));
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

        #endregion
    }
}