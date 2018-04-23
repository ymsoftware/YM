using Microsoft.VisualStudio.TestTools.UnitTesting;
using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.TermQueries;
using YM.Json;

namespace YM.Elasticsearch.Tests
{
    [TestClass]
    public class TemQueriesTests
    {
        [TestMethod]
        public void Term_query_must_transform_correclty_to_ES()
        {
            var query = new TermQuery("type", "text");
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Value.ToString() == "text");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.TermQuery);
            query = q as TermQuery;
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Value.ToString() == "text");
        }

        [TestMethod]
        public void Terms_query_must_transform_correclty_to_ES()
        {
            var query = new TermsQuery("type", new string[] { "text", "photo" });
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Values[0].ToString() == "text");
            Assert.IsTrue(query.Values[1].ToString() == "photo");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.TermsQuery);
            query = q as TermsQuery;
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Values[0].ToString() == "text");
            Assert.IsTrue(query.Values[1].ToString() == "photo");
        }

        [TestMethod]
        public void Prefix_query_must_transform_correclty_to_ES()
        {
            var query = new PrefixQuery("type", "te");
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Value.ToString() == "te");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.PrefixQuery);
            query = q as PrefixQuery;
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Value.ToString() == "te");
        }

        [TestMethod]
        public void Wildcrad_query_must_transform_correclty_to_ES()
        {
            var query = new WildcardQuery("type", "te*t");
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Value.ToString() == "te*t");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.WildcardQuery);
            query = q as WildcardQuery;
            Assert.IsTrue(query.Field == "type");
            Assert.IsTrue(query.Value.ToString() == "te*t");
        }

        [TestMethod]
        public void Range_query_must_transform_correclty_to_ES()
        {
            var query = new RangeQuery("date", "now-1d", "now");
            Assert.IsTrue(query.Field == "date");
            Assert.IsTrue(query.Values.From.Value.ToString() == "now-1d");
            Assert.IsTrue(query.Values.To.Value.ToString() == "now");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.RangeQuery);
            query = q as RangeQuery;
            Assert.IsTrue(query.Field == "date");
            Assert.IsTrue(query.Values.From.Value.ToString() == "now-1d");
            Assert.IsTrue(query.Values.To.Value.ToString() == "now");

            query = new RangeQuery("date", new RangeValues(new RangeValue("now-1d", false), null));
            Assert.IsTrue(query.Field == "date");
            Assert.IsTrue(query.Values.From.Value.ToString() == "now-1d");
            Assert.IsFalse(query.Values.From.Include);

            json = query.ToString();
            jo = JsonObject.Parse(json);
            q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.RangeQuery);
            query = q as RangeQuery;
            Assert.IsTrue(query.Field == "date");
            Assert.IsTrue(query.Values.From.Value.ToString() == "now-1d");
            Assert.IsFalse(query.Values.From.Include);
        }

        [TestMethod]
        public void Exists_query_must_transform_correclty_to_ES()
        {
            var query = new ExistsQuery("type");
            Assert.IsTrue(query.Field == "field");
            Assert.IsTrue(query.Value.ToString() == "type");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.ExistsQuery);
            query = q as ExistsQuery;
            Assert.IsTrue(query.Field == "field");
            Assert.IsTrue(query.Value.ToString() == "type");
        }
    }
}