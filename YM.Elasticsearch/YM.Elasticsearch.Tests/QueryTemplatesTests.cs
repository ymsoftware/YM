using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using YM.Elasticsearch.Query;
using YM.Json;

namespace YM.Elasticsearch.Tests
{
    [TestClass]
    public class QueryTemplatesTests
    {
        [TestMethod]
        public void test_templates()
        {
            var tests = JsonArray.Parse(File.ReadAllText("test_templates.json"))
                .Select(e => e.Get<JsonObject>())
                .ToArray();

            foreach(var test in tests)
            {
                var template = test.Property<JsonObject>("template");
                var parameters = test.Property<JsonObject>("params").ToDictionary();
                var output = test.Property<JsonObject>("output");
                var query = template.SetParameters(parameters);

                if (query == null || query.IsEmpty)
                {
                    Assert.IsTrue(output == null || output.IsEmpty);
                }
                else
                {
                    var q = query.ToQuery();
                    query = q.ToJson();
                    Assert.IsTrue(query.ToString(false) == output.ToString(false));
                }
            }            
        }
    }
}