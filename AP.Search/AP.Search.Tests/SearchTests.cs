using AP.Search.SearchTemplates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YM.Elasticsearch.Client;
using YM.Json;

namespace AP.Search.Tests
{
    [TestClass]
    public class SearchTests
    {
        [TestMethod]
        public async Task test_templates()
        {
            var tests = JsonArray.Parse(File.ReadAllText("test_templates.json"))
                .Select(e => e.Get<JsonObject>())
                .ToArray();

            var es = new ElasticsearchClient();
            await es.DeleteIndexAsync("search_templates");

            var repo = new SearchTemplateRepository(new SearchTemplateCache());

            foreach (var test in tests)
            {
                var template = new SearchTemplate(test.Property<JsonObject>("template"));
                string id = template.Id;

                var idr = await repo.SaveTemplateAsync(template, true);
                Assert.IsTrue(idr.IsSuccess);
                
                template = await repo.GetTemplateAsync(id);
                Assert.IsTrue(template.Id == id);

                var sr = await repo.SearchTemplatesAsync("q=id:" + id);
                Assert.IsTrue(sr.Hits.Total == 1);

                var ddr = await repo.DeleteTemplateAsync(id, true);
                Assert.IsTrue(ddr.IsSuccess);

                sr = await repo.SearchTemplatesAsync("q=id:" + id);
                Assert.IsTrue(sr.Hits.Total == 0);

                idr = await repo.SaveTemplateAsync(template);
                Assert.IsTrue(idr.IsSuccess);

                template = await repo.GetTemplateAsync(id);
                Assert.IsTrue(template.Id == id);

                var parameters = test.Property<JsonObject>("params").ToDictionary();
                var output = test.Property<JsonObject>("output");
                var search = template.GetSearchRequest(parameters).ToJson();

                if (search == null || search.IsEmpty)
                {
                    Assert.IsTrue(output == null || output.IsEmpty);
                }
                else
                {
                    Assert.IsTrue(search.ToString(false) == output.ToString(false));
                }
            }
        }
    }
}
