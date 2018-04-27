using AP.Search.QueryExpansion;
using AP.Search.SearchTemplates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YM.Elasticsearch;
using YM.Elasticsearch.Client;
using YM.Elasticsearch.Query;
using YM.Json;

namespace AP.Search.Tests
{
    [TestClass]
    public class SearchTests
    {
        [TestMethod]
        public async Task Templates_must_be_transformed_to_correct_outputs()
        {
            //var temp = new Temp();
            //await temp.Test();

            var tests = JsonArray.Parse(File.ReadAllText("test_templates.json"))
                .Select(e => e.Get<JsonObject>())
                .ToArray();

            var es = new ElasticsearchClient();
            await es.DeleteIndexAsync("search_templates");

            var repo = new SearchTemplateRepository(CacheExpiration.Sliding(60 * 1000));

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
                var search = template.GetSearchRequest(parameters);
                var jo = search.ToJson();

                if (jo == null || jo.IsEmpty)
                {
                    Assert.IsTrue(output == null || output.IsEmpty);
                }
                else
                {
                    Assert.IsTrue(search.Index.Length > 0);
                    Assert.IsFalse(search.Index.StartsWith("$"));
                    Assert.IsTrue(jo.ToString(false) == output.ToString(false));
                }
            }

            repo = new SearchTemplateRepository(CacheExpiration.Sliding(1000));
            foreach (var test in tests.Take(1))
            {
                var jo = test.Property<JsonObject>("template");
                var template = new SearchTemplate(jo);
                string id = template.Id;
                string name = template.Name;

                var idr = await repo.SaveTemplateAsync(template, true);
                Assert.IsTrue(idr.IsSuccess);

                template = await repo.GetTemplateAsync(id);
                Assert.IsTrue(template.Id == id);
                Assert.IsTrue(template.Name == name);

                string @new = name + "-updated";
                jo.Remove("name").Add("name", @new);
                template = new SearchTemplate(jo);
                idr = await repo.SaveTemplateAsync(template, true);
                Assert.IsTrue(idr.IsSuccess);

                Thread.Sleep(1010);

                template = await repo.GetTemplateAsync(id);
                Assert.IsTrue(template.Id == id);
                Assert.IsTrue(template.Name == @new);
            }
        }

        [TestMethod]
        public void Queries_must_be_transformed_to_correct_outputs()
        {
            var tests = JsonArray.Parse(File.ReadAllText("test_query_transform.json"))
                .Select(e => e.Get<JsonObject>())
                .ToArray();

            foreach (var test in tests)
            {
                var jo = test.Property<JsonObject>("input");
                var input = jo.ToQuery();

                var aliases = FieldAliasesRepository.GetAliases(jo.Property<string>("field_aliases"));
                var expander = QueryExpanderRepository.GetQueryExpander(jo.Property<string>("query_expand"));

                jo = test.Property<JsonObject>("output");
                var output = jo.ToQuery();

                var query = input.Transform(aliases, expander);
                Assert.IsTrue(query.ToJson().ToString(false) == output.ToJson().ToString(false));
            }
        }
    }
}