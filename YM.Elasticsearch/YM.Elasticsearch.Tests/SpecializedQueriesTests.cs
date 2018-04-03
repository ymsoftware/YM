using Microsoft.VisualStudio.TestTools.UnitTesting;
using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.SpecializedQueries;
using YM.Json;

namespace YM.Elasticsearch.Tests
{
    [TestClass]
    public class SpecializedQueriesTests
    {
        [TestMethod]
        public void percolate_query()
        {
            var document = new JsonObject().Add("headline", "YM");
            var query = new PercolateQuery("query", document);
            Assert.IsTrue(query.Field == "query");
            Assert.IsTrue(query.Document.Property<string>("headline") == "YM");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.PercolateQuery);
            query = q as PercolateQuery;
            Assert.IsTrue(query.Field == "query");
            Assert.IsTrue(query.Document.Property<string>("headline") == "YM");

            var cs = new ConstantScoreQuery(query);
            query = cs.Query as PercolateQuery;
            Assert.IsTrue(query.Field == "query");
            Assert.IsTrue(query.Document.Property<string>("headline") == "YM");

            json = cs.ToString();
            jo = JsonObject.Parse(json);
            q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.ConstantScoreQuery);
            query = (q as ConstantScoreQuery).Query as PercolateQuery;
            Assert.IsTrue(query.Field == "query");
            Assert.IsTrue(query.Document.Property<string>("headline") == "YM");
        }
    }
}