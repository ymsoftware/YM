﻿namespace YM.Elasticsearch.Query
{
    public enum QueryType
    {
        MatchAllQuery,
        TermQuery,
        TermsQuery,
        PrefixQuery,
        WildcardQuery,
        RangeQuery,
        ExistsQuery,
        MatchQuery,
        MatchPhraseQuery,
        MatchPhrasePrefixQuery,
        QueryStringQuery,
        SimpleQueryStringQuery,
        MultiMatchQuery,
        BoolQuery,
        ConstantScoreQuery,        
        FilterScoreFunction,
        DecayScoreFunction,
        FunctionScoreQuery,
        PercolateQuery
    }
}