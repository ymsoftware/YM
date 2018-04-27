using AP.Search.QueryExpansion;
using System;
using System.Collections.Generic;
using System.Linq;
using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Elasticsearch.Query.FullTextQueries.QueryString;
using YM.Elasticsearch.Query.JoiningQueries;
using YM.Elasticsearch.Query.TermQueries;

namespace AP.Search
{
    public static class QueryExtensions
    {
        public static IQuery Transform(this IQuery query, IDictionary<string, string[]> aliases, IQueryExpander qe)
        {
            if (query is BoolQuery)
            {
                return Transform(query as BoolQuery, aliases, qe);
            }

            if (query is MultiMatchQuery)
            {
                return Transform(query as MultiMatchQuery, aliases, qe);
            }

            if (query is QueryStringQuery)
            {
                return Transform(query as QueryStringQuery, aliases, qe);
            }

            if (query is TermQuery)
            {
                var q = query as TermQuery;
                return SetAliases(q, aliases, (f) => new TermQuery(f, q.Value));
            }

            if (query is TermsQuery)
            {
                var q = query as TermsQuery;
                return SetAliases(q, aliases, (f) => new TermsQuery(f, q.Values));
            }

            if (query is RangeQuery)
            {
                var q = query as RangeQuery;
                return SetAliases(q, aliases, (f) => new RangeQuery(f, q.Values));
            }

            if (query is MatchQuery)
            {
                var q = query as MatchQuery;
                string value = (string)q.Value;
                return SetAliases(q, aliases, (f) => new MatchQuery(f, value, q.IsAnd, q.IsZeroTerms));
            }

            if (query is PrefixQuery)
            {
                var q = query as PrefixQuery;
                string value = (string)q.Value;
                return SetAliases(q, aliases, (f) => new PrefixQuery(f, value));
            }

            if (query is WildcardQuery)
            {
                var q = query as WildcardQuery;
                string value = (string)q.Value;
                return SetAliases(q, aliases, (f) => new WildcardQuery(f, value));
            }

            if (query is ExistsQuery)
            {
                var q = query as ExistsQuery;
                return SetAliases(q, aliases, (f) => new ExistsQuery(f));
            }

            if (query is MatchPhrasePrefixQuery)
            {
                var q = query as MatchPhrasePrefixQuery;
                string value = (string)q.Value;
                return SetAliases(q, aliases, (f) => new MatchPhrasePrefixQuery(f, value));
            }

            if (query is MatchPhraseQuery)
            {
                var q = query as MatchPhraseQuery;
                string value = (string)q.Value;
                return SetAliases(q, aliases, (f) => new MatchPhraseQuery(f, value));
            }

            if (query is FunctionScoreQuery)
            {
                return Transform(query as FunctionScoreQuery, aliases, qe);
            }

            if (query is ConstantScoreQuery)
            {
                return Transform(query as ConstantScoreQuery, aliases, qe);
            }

            if (query is NestedQuery)
            {
                var q = query as NestedQuery;
                return new NestedQuery(q.Path, q.Query.Transform(aliases, qe));
            }

            return query;
        }

        public static QueryString Transform(this QueryString qs, IDictionary<string, string[]> aliases, ref bool isQSQ, ref bool isSQSQ)
        {
            var tokens = new List<QueryStringToken>();

            foreach (var token in qs.Tokens)
            {
                switch (token.Type)
                {
                    case QueryStringTokenType.Query:
                        tokens.Add(SetAliases(token as QueryStringQueryToken, aliases));
                        isQSQ = true;
                        break;
                    case QueryStringTokenType.Boost:
                    case QueryStringTokenType.Fuzzy:
                        tokens.Add(token);
                        isQSQ = true;
                        break;
                    case QueryStringTokenType.And:
                    case QueryStringTokenType.Or:
                    case QueryStringTokenType.Not:
                    case QueryStringTokenType.Must:
                    case QueryStringTokenType.MustNot:
                        tokens.Add(token);
                        isSQSQ = true;
                        break;
                    case QueryStringTokenType.Group:
                        bool qsq = false;
                        bool sqsq = false;
                        tokens.AddRange(((QueryStringGroupToken)token).Group.Transform(aliases, ref qsq, ref sqsq).Tokens);
                        if (qsq) isQSQ = true;
                        if (sqsq) isSQSQ = true;
                        break;
                    default:
                        tokens.Add(token);
                        break;
                }
            }

            return new QueryString(tokens.ToArray());
        }

        private static IQuery Transform(MultiMatchQuery query, IDictionary<string, string[]> aliases, IQueryExpander qe)
        {
            bool qsq = false;
            bool sqsq = false;

            var qs = query.Query.ToQueryString().Transform(aliases, ref qsq, ref sqsq);

            if (qsq)
            {
                return new QueryStringQuery(qs.ToString(), query.Fields, query.IsAnd);
            }

            if (sqsq)
            {
                return new SimpleQueryStringQuery(qs.ToString(), query.Fields, query.IsAnd);
            }

            return qe == null ? query : qe.Expand(query);
        }

        private static IQuery Transform(QueryStringQuery query, IDictionary<string, string[]> aliases, IQueryExpander qe)
        {
            bool qsq = false;
            bool sqsq = false;

            var qs = query.Query.ToQueryString().Transform(aliases, ref qsq, ref sqsq);

            if (qsq)
            {
                return new QueryStringQuery(qs.ToString(), query.Fields, query.IsAnd);
            }
            else
            {
                if (sqsq)
                {
                    return new SimpleQueryStringQuery(qs.ToString(), query.Fields, query.IsAnd);
                }
                else
                {
                    var terms = qs.GetTerms();
                    if (terms.Length == 0)
                    {
                        return new MatchAllQuery();
                    }
                    else
                    {
                        var match = new MultiMatchQuery(string.Join(" ", terms), query.Fields, MultiMatchType.BestFields, query.IsAnd);
                        return qe == null ? match : qe.Expand(match);
                    }
                }
            }
        }

        private static IQuery Transform(BoolQuery query, IDictionary<string, string[]> aliases, IQueryExpander qe)
        {
            var must = query.MustQueries == null || query.MustQueries.Length == 0 ? null : query.MustQueries.Select(e => e.Transform(aliases, qe)).ToArray();
            var not = query.NotQueries == null || query.NotQueries.Length == 0 ? null : query.NotQueries.Select(e => e.Transform(aliases, qe)).ToArray();
            var filter = query.FilterQueries == null || query.FilterQueries.Length == 0 ? null : query.FilterQueries.Select(e => e.Transform(aliases, qe)).ToArray();
            var should = query.ShouldQueries == null || query.ShouldQueries.Length == 0 ? null : query.ShouldQueries.Select(e => e.Transform(aliases, qe)).ToArray();
            return new BoolQuery(must, not, should, filter);
        }

        private static IQuery Transform(ConstantScoreQuery query, IDictionary<string, string[]> aliases, IQueryExpander qe)
        {
            return new ConstantScoreQuery(query.Query.Transform(aliases, qe));
        }

        private static IQuery Transform(FunctionScoreQuery query, IDictionary<string, string[]> aliases, IQueryExpander qe)
        {
            return new FunctionScoreQuery(query.Query.Transform(aliases, qe), query.Functions);
        }

        private static QueryStringToken SetAliases(QueryStringQueryToken token, IDictionary<string, string[]> aliases)
        {
            if (aliases == null || aliases.Count == 0)
            {
                return token;
            }

            if (token.Query is MatchQuery)
            {
                var match = token.Query as MatchQuery;

                if (aliases.TryGetValue(match.Field, out string[] fields))
                {
                    string value = (string)match.Value;

                    if (fields.Length == 1)
                    {
                        return new QueryStringQueryToken(new MatchQuery(fields[0], value));
                    }
                    else if (fields.Length > 1)
                    {
                        var tokens = new List<QueryStringToken>();

                        foreach (string field in fields)
                        {
                            if (tokens.Count > 0)
                            {
                                tokens.Add(new QueryStringToken(QueryStringTokenType.Or));
                            }

                            tokens.Add(new QueryStringQueryToken(new MatchQuery(field, value)));
                        }

                        return new QueryStringGroupToken(new QueryString(tokens.ToArray()));
                    }
                }
            }
            else if (token.Query is RangeQuery)
            {
                var range = token.Query as RangeQuery;

                if (aliases.TryGetValue(range.Field, out string[] fields))
                {
                    var values = range.Values;

                    if (fields.Length == 1)
                    {
                        return new QueryStringQueryToken(new RangeQuery(fields[0], values));
                    }
                    else if (fields.Length > 1)
                    {
                        var tokens = new List<QueryStringToken>();

                        foreach (string field in fields)
                        {
                            if (tokens.Count > 0)
                            {
                                tokens.Add(new QueryStringToken(QueryStringTokenType.Or));
                            }

                            tokens.Add(new QueryStringQueryToken(new RangeQuery(field, values)));
                        }

                        return new QueryStringGroupToken(new QueryString(tokens.ToArray()));
                    }
                }
            }

            return token;
        }

        private static IQuery SetAliases<T>(T query, IDictionary<string, string[]> aliases, Func<string, IQuery> create)
            where T : FieldQueryBase
        {
            if (aliases == null || aliases.Count == 0)
            {
                return query;
            }

            if (aliases.TryGetValue(query.Field, out string[] fields))
            {
                if (fields.Length == 1)
                {
                    return create(fields[0]);
                }
                else if (fields.Length > 1)
                {
                    return new BoolQuery().Must(fields.Select(e => create(fields[0])).ToArray());
                }
            }

            return query;
        }
    }
}