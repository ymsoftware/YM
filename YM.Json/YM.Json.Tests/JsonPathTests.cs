using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YM.Json.Tests
{
    [TestClass]
    public class JsonPathTests
    {
        [TestMethod]
        public void json_paths()
        {
            string json = File.ReadAllText(@"TestFiles\json_paths.json");
            var jo = JsonObject.Parse(json);
            json = jo.ToString();

            Assert.IsTrue(jo.PathValue<int>("$.size") == 100);
            Assert.IsTrue(jo.PathValue<int>("size") == 100);
            Assert.IsTrue(jo.PathValue<string>("$.query.bool.must.match.headline") == "test");
            Assert.IsTrue(jo.PathValue<string>("query.bool.must.match.headline") == "test");
            Assert.IsTrue(jo.PathValue<string>("$.query.bool.filter[0].term.type") == "text");
            Assert.IsTrue(jo.PathValue<JsonObject>("$.query.bool.filter[1].terms").Property<JsonArray>("filings.products").Get<int>(0) == 1);

            var ja = jo.PathValues<JsonObject>("query.bool.filter[]");
            Assert.IsTrue(ja.Length == 2);
            Assert.IsTrue(ja[1].Property<JsonObject>("terms").Property<JsonArray>("filings.products").Get<long>(1) == 2);
        }
    }
}
