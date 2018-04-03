namespace YM.Elasticsearch.Query.FullTextQueries
{
    public enum MultiMatchType
    {
        BestFields, //best_fields (default): Finds documents which match any field, but uses the _score from the best field. See best_fields.
        MostFields, //most_fields: Finds documents which match any field and combines the _score from each field. See most_fields.
        CrossFields, //cross_fields: Treats fields with the same analyzer as though they were one big field. Looks for each word in any field. See cross_fields.
        Phrase, //phrase: Runs a match_phrase query on each field and combines the _score from each field. See phrase and phrase_prefix.
        PhrasePrefix //phrase_prefix: Runs a match_phrase_prefix query on each field and combines the _score from each field
    }
}
