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
        public void match_query()
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
        public void match_phrase_query()
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
        public void match_prefix_phrase_query()
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
        public void query_string_query()
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
        public void simple_query_string_query()
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
        public void multi_match_query()
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