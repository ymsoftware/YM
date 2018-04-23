using Microsoft.VisualStudio.TestTools.UnitTesting;
using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Json;

namespace YM.Elasticsearch.Tests
{
    [TestClass]
    public class FullTextQueriesTests
    {
        [TestMethod]
        public void Match_query_must_transform_correclty_to_ES()
        {
            var query = new MatchQuery("headline", "YM");
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "YM");
            Assert.IsFalse(query.IsAnd);
            Assert.IsFalse(query.IsZeroTerms);

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.MatchQuery);
            query = q as MatchQuery;
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "YM");
            Assert.IsFalse(query.IsAnd);
            Assert.IsFalse(query.IsZeroTerms);

            query = new MatchQuery("headline", "Yuri Metelkin", true, true);
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.IsZeroTerms);

            json = query.ToString();
            jo = JsonObject.Parse(json);
            q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.MatchQuery);
            query = q as MatchQuery;
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.IsZeroTerms);
        }

        [TestMethod]
        public void Match_phrase_query_must_transform_correclty_to_ES()
        {
            var query = new MatchPhraseQuery("headline", "Yuri Metelkin");
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "Yuri Metelkin");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.MatchPhraseQuery);
            query = q as MatchPhraseQuery;
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "Yuri Metelkin");
        }

        [TestMethod]
        public void Match_prefix_phrase_query_must_transform_correclty_to_ES()
        {
            var query = new MatchPhrasePrefixQuery("headline", "Yuri M");
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "Yuri M");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.MatchPhrasePrefixQuery);
            query = q as MatchPhrasePrefixQuery;
            Assert.IsTrue(query.Field == "headline");
            Assert.IsTrue(query.Value.ToString() == "Yuri M");
        }

        [TestMethod]
        public void Query_string_query_must_transform_correclty_to_ES()
        {
            var query = new QueryStringQuery("Yuri Metelkin", new string[] { "head" }, true);
            Assert.IsTrue(query.Query == "Yuri Metelkin");
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.QueryStringQuery);
            query = q as QueryStringQuery;
            Assert.IsTrue(query.Query == "Yuri Metelkin");
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");

            query = new QueryStringQuery("head:YM AND body:SV");
            Assert.IsTrue(query.Query == "head:YM AND body:SV");
            Assert.IsFalse(query.IsAnd);
            Assert.IsNull(query.Fields);

            json = query.ToString();
            jo = JsonObject.Parse(json);
            q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.QueryStringQuery);
            query = q as QueryStringQuery;
            Assert.IsTrue(query.Query == "head:YM AND body:SV");
            Assert.IsFalse(query.IsAnd);
            Assert.IsNull(query.Fields);
        }

        [TestMethod]
        public void Simple_query_string_query_must_transform_correclty_to_ES()
        {
            var query = new SimpleQueryStringQuery("\"fried eggs\" +(eggplant | potato) -frittata", new string[] { "head" }, true);
            Assert.IsTrue(query.Query == "\"fried eggs\" +(eggplant | potato) -frittata");
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.SimpleQueryStringQuery);
            query = q as SimpleQueryStringQuery;
            Assert.IsTrue(query.Query == "\"fried eggs\" +(eggplant | potato) -frittata");
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");
        }

        [TestMethod]
        public void Multi_match_query_must_transform_correclty_to_ES()
        {
            var query = new MultiMatchQuery("Yuri Metelkin", new string[] { "head", "body" }, MultiMatchType.MostFields, true, 0.3);
            Assert.IsTrue(query.Query == "Yuri Metelkin");
            Assert.IsTrue(query.MatchType == MultiMatchType.MostFields);
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");
            Assert.IsTrue(query.Fields[1] == "body");
            Assert.IsTrue(query.TieBreaker == 0.3);

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.MultiMatchQuery);
            query = q as MultiMatchQuery;
            Assert.IsTrue(query.Query == "Yuri Metelkin");
            Assert.IsTrue(query.MatchType == MultiMatchType.MostFields);
            Assert.IsTrue(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");
            Assert.IsTrue(query.Fields[1] == "body");
            Assert.IsTrue(query.TieBreaker == 0.3);

            query = new MultiMatchQuery("Yuri Metelkin", new string[] { "head", "body" });
            Assert.IsTrue(query.Query == "Yuri Metelkin");
            Assert.IsTrue(query.MatchType == MultiMatchType.BestFields);
            Assert.IsFalse(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");
            Assert.IsTrue(query.Fields[1] == "body");
            Assert.IsTrue(query.TieBreaker == 0);

            json = query.ToString();
            jo = JsonObject.Parse(json);
            q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.MultiMatchQuery);
            query = q as MultiMatchQuery;
            Assert.IsTrue(query.Query == "Yuri Metelkin");
            Assert.IsTrue(query.MatchType == MultiMatchType.BestFields);
            Assert.IsFalse(query.IsAnd);
            Assert.IsTrue(query.Fields[0] == "head");
            Assert.IsTrue(query.Fields[1] == "body");
            Assert.IsTrue(query.TieBreaker == 0);
        }
    }
}