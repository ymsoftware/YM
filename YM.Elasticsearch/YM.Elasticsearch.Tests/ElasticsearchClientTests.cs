using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using YM.Elasticsearch.Client;
using YM.Elasticsearch.Client.Documents;
using YM.Elasticsearch.Client.Indices;
using YM.Elasticsearch.Client.Search;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Elasticsearch.Query.TermQueries;
using YM.Json;

namespace YM.Elasticsearch.Tests
{
    [TestClass]
    public class ElasticsearchClientTests
    {
        [TestMethod]
        public async Task Cluster_must_return_correct_version()
        {
            var es = new ElasticsearchClient();
            var cr = await es.GetClusterAsync();
            Assert.IsTrue(cr.Name == "elasticsearch");
            Assert.IsTrue(cr.VersionNumber > 6);
        }

        [TestMethod]
        public async Task Document_crud_must_complete_all_crud_operations()
        {
            var es = new ElasticsearchClient();

            await es.DeleteIndexAsync("test");

            var cir = await es.CreateIndexAsync("test");
            Assert.IsTrue(cir.IsSuccess);
            Assert.IsTrue(cir.Index == "test");

            var source = new JsonObject()
                .Add("name", "YM");

            var document = new ElasticsearchDocument("1", source, "test");
            var idr = await es.IndexDocumentAsync(new IndexDocumentRequest(document, true));
            Assert.IsTrue(idr.IsSuccess);
            Assert.IsTrue(idr.Result == "created");
            Assert.IsTrue(idr.Index == "test");
            Assert.IsTrue(idr.Id == "1");
            Assert.IsTrue(idr.Version == 1);

            var gdr = await es.GetDocumentAsync(new GetDocumentRequest("1", "test"));
            Assert.IsTrue(gdr.IsSuccess);
            Assert.IsTrue(gdr.Document.Id == "1");
            Assert.IsTrue(gdr.Document.Index == "test");
            Assert.IsTrue(gdr.Document.Version == 1);

            idr = await es.IndexDocumentAsync(new IndexDocumentRequest(document, true));
            Assert.IsTrue(idr.IsSuccess);
            Assert.IsTrue(idr.Result == "updated");
            Assert.IsTrue(idr.Index == "test");
            Assert.IsTrue(idr.Id == "1");
            Assert.IsTrue(idr.Version == 2);

            gdr = await es.GetDocumentAsync(new GetDocumentRequest("1", "test"));
            Assert.IsTrue(gdr.IsSuccess);
            Assert.IsTrue(gdr.Document.Version == 2);

            var ddr = await es.DeleteDocumentAsync(new DeleteDocumentRequest("1", "test", "doc", true));
            Assert.IsTrue(ddr.IsSuccess);
            Assert.IsTrue(ddr.Id == "1");
            Assert.IsTrue(ddr.Index == "test");
            Assert.IsTrue(ddr.Version == 3);

            ddr = await es.DeleteDocumentAsync(new DeleteDocumentRequest("1", "test", "doc", true));
            Assert.IsFalse(ddr.IsSuccess);
            Assert.IsTrue(ddr.Id == "1");
            Assert.IsTrue(ddr.Index == "test");
            Assert.IsTrue(ddr.Version == 4);
            Assert.IsTrue(ddr.Result == "not_found");

            gdr = await es.GetDocumentAsync(new GetDocumentRequest("1", "test"));
            Assert.IsFalse(gdr.IsSuccess);
            Assert.IsTrue(gdr.Document.Id == "1");
            Assert.IsTrue(gdr.Document.Index == "test");
            Assert.IsTrue(gdr.Document.Version == 0);
            Assert.IsNull(gdr.Document.Source);
        }

        [TestMethod]
        public async Task Alias_must_map_correctly()
        {
            var es = new ElasticsearchClient();

            await es.DeleteIndexAsync("test");

            var cir = await es.CreateIndexAsync("test");
            Assert.IsTrue(cir.IsSuccess);
            Assert.IsTrue(cir.Index == "test");

            var source = new JsonObject()
                .Add("name", "YM");

            var document = new ElasticsearchDocument("1", source, "test");
            var idr = await es.IndexDocumentAsync(new IndexDocumentRequest(document, true));

            var search = new SearchRequest("test")
                .SetQuery(new MatchQuery("name", "YM"));

            var hits = await es.SearchAsync(search);
            Assert.IsTrue(hits.Hits.Total == 1);
            var hit = hits.Hits.Hits[0];
            Assert.IsTrue(hit.Id == "1");

            var response = await es.AliasAsync(new AliasRequest().Add("test", "test-alias"));
            Assert.IsTrue(response.IsSuccess);

            var aliases = await es.GetIndexAliasesAsync("test");
            Assert.IsTrue(aliases.Aliases[0] == "test-alias");
            var indices = await es.GetAliasIndicesAsync("test-alias");
            Assert.IsTrue(indices.Indices[0] == "test");

            search = new SearchRequest("test-alias")
                .SetQuery(new MatchQuery("name", "YM"));

            hits = await es.SearchAsync(search);
            Assert.IsTrue(hits.Hits.Total == 1);
            hit = hits.Hits.Hits[0];
            Assert.IsTrue(hit.Id == "1");

            response = await es.AliasAsync(new AliasRequest().Remove("test", "test-alias"));
            Assert.IsTrue(response.IsSuccess);

            search = new SearchRequest("test-alias")
                .SetQuery(new MatchQuery("name", "YM"));

            hits = await es.SearchAsync(search);
            Assert.IsNull(hits.Hits);
        }

        [TestMethod]
        public async Task Bulk_document_crud_must_process_all_documents_correctly()
        {
            var es = new ElasticsearchClient();

            await es.DeleteIndexAsync("test");

            var cir = await es.CreateIndexAsync("test");
            Assert.IsTrue(cir.IsSuccess);
            Assert.IsTrue(cir.Index == "test");

            var documents = new ElasticsearchDocument[] {
                new ElasticsearchDocument("1", new JsonObject().Add("name", "YM"), "test"),
                new ElasticsearchDocument("2", new JsonObject().Add("name", "SV"), "test")
            };
            var br = await es.BulkIndexDocumentsAsync(documents);
            Assert.IsTrue(br.IsSuccess);

            var bulk = new BulkRequest(true).Add(documents.Select(e => new BulkRequestItem("index", e.Id, e.Index, e.Type, e.Source)));
            br = await es.BulkDocumentsAsync(bulk);
            Assert.IsTrue(br.IsSuccess);

            var gdr = await es.GetDocumentAsync(new GetDocumentRequest("1", "test"));
            Assert.IsTrue(gdr.IsSuccess);
            Assert.IsTrue(gdr.Document.Id == "1");
            Assert.IsTrue(gdr.Document.Index == "test");
            Assert.IsTrue(gdr.Document.Version == 2);

            bulk = new BulkRequest(true).Add(documents.Select(e => new BulkRequestItem("delete", e.Id, e.Index, e.Type, null)));
            br = await es.BulkDocumentsAsync(bulk);
            Assert.IsTrue(br.IsSuccess);

            gdr = await es.GetDocumentAsync(new GetDocumentRequest("1", "test"));
            Assert.IsFalse(gdr.IsSuccess);
            Assert.IsTrue(gdr.Document.Id == "1");
            Assert.IsTrue(gdr.Document.Index == "test");
        }

        [TestMethod]
        public async Task Search_request_must_transform_to_correct_ES_request()
        {
            var es = new ElasticsearchClient();

            await es.DeleteIndexAsync("test");

            var cir = await es.CreateIndexAsync("test");
            Assert.IsTrue(cir.IsSuccess);
            Assert.IsTrue(cir.Index == "test");

            var documents = new ElasticsearchDocument[] {
                new ElasticsearchDocument("1", new JsonObject().Add("id", 1).Add("name", "YM"), "test"),
                new ElasticsearchDocument("2", new JsonObject().Add("id", 2).Add("name", "SV"), "test")
            };
            var br = await es.BulkDocumentsAsync(new BulkRequest(true).Index(documents));
            Assert.IsTrue(br.IsSuccess);

            var search = new SearchRequest("test")
                .SetQuery(new MatchQuery("name", "YM"));

            var hits = await es.SearchAsync(search);
            Assert.IsTrue(hits.Hits.Total == 1);

            var hit = hits.Hits.Hits[0];
            Assert.IsTrue(hit.Id == "1");
            Assert.IsTrue(hit.Source.Property<int>("id") == 1);

            search = new SearchRequest("test")
                .SetQuery(new TermQuery("id", 2));

            hits = await es.SearchAsync(search);
            Assert.IsTrue(hits.Hits.Total == 1);

            hit = hits.Hits.Hits[0];
            Assert.IsTrue(hit.Id == "2");
            Assert.IsTrue(hit.Source.Property<string>("name") == "SV");

            search = new SearchRequest("test")
                .SetQuery(new ConstantScoreQuery(new RangeQuery("id", 1, null)))
                .SetFrom(1)
                .SetSize(1)
                .HideSource();

            hits = await es.SearchAsync(search);
            Assert.IsTrue(hits.Hits.Total == 2);
            Assert.IsTrue(hits.Hits.Hits.Length == 1);

            hit = hits.Hits.Hits[0];
            Assert.IsTrue(hit.Score == 1);
            Assert.IsNull(hit.Source);
        }

        [TestMethod]
        public async Task Scroll_request_must_scroll_correctly()
        {
            var es = new ElasticsearchClient();

            await es.DeleteIndexAsync("test");

            var cir = await es.CreateIndexAsync("test");
            Assert.IsTrue(cir.IsSuccess);
            Assert.IsTrue(cir.Index == "test");

            var documents = new ElasticsearchDocument[] {
                new ElasticsearchDocument("1", new JsonObject().Add("id", 1).Add("name", "YM"), "test"),
                new ElasticsearchDocument("2", new JsonObject().Add("id", 2).Add("name", "SV"), "test")
            };
            var br = await es.BulkDocumentsAsync(new BulkRequest(true).Index(documents));
            Assert.IsTrue(br.IsSuccess);

            var request = new SearchRequest("test")
                .SetSort("_doc")
                .SetSize(1);

            var scroll = new ScrollRequest(request);
            var response = await es.ScrollAsync(scroll);
            Assert.IsFalse(response.IsEmpty);
            Assert.IsTrue(response.Hits.Total == 2);
            Assert.IsTrue(response.Hits.Hits.Length == 1);
            Assert.IsNotNull(response.ScrollId);

            scroll = new ScrollRequest(response.ScrollId);
            response = await es.ScrollAsync(scroll);
            Assert.IsFalse(response.IsEmpty);
            Assert.IsTrue(response.Hits.Total == 2);
            Assert.IsTrue(response.Hits.Hits.Length == 1);
            Assert.IsNotNull(response.ScrollId);

            scroll = new ScrollRequest(response.ScrollId);
            response = await es.ScrollAsync(scroll);
            Assert.IsTrue(response.IsEmpty);
            Assert.IsTrue(response.Hits.Total == 2);            
            Assert.IsNotNull(response.ScrollId);
        }

        [TestMethod]
        public async Task Search_after_request_must_return_correct_results()
        {
            var es = new ElasticsearchClient();

            await es.DeleteIndexAsync("test");

            var cir = await es.CreateIndexAsync("test");
            Assert.IsTrue(cir.IsSuccess);
            Assert.IsTrue(cir.Index == "test");

            var documents = new ElasticsearchDocument[] {
                new ElasticsearchDocument("1", new JsonObject().Add("id", 1).Add("name", "YM"), "test"),
                new ElasticsearchDocument("2", new JsonObject().Add("id", 2).Add("name", "SV"), "test")
            };
            var br = await es.BulkDocumentsAsync(new BulkRequest(true).Index(documents));
            Assert.IsTrue(br.IsSuccess);

            var request = new SearchRequest("test")
                .SetSort("id")
                .SetSize(1);

            var response = await es.SearchAfterAsync(request);
            Assert.IsFalse(response.IsEmpty);
            Assert.IsTrue(response.Hits.Total == 2);
            Assert.IsTrue(response.Hits.Hits.Length == 1);
            Assert.IsNotNull(response.SearchAfter);

            request.SetSearchAfter(response.SearchAfter);
            response = await es.SearchAfterAsync(request);
            Assert.IsFalse(response.IsEmpty);
            Assert.IsTrue(response.Hits.Total == 2);
            Assert.IsTrue(response.Hits.Hits.Length == 1);
            Assert.IsNotNull(response.SearchAfter);

            request.SetSearchAfter(response.SearchAfter);
            response = await es.SearchAfterAsync(request);
            Assert.IsTrue(response.IsEmpty);
            Assert.IsTrue(response.Hits.Total == 2);
            Assert.IsNull(response.SearchAfter);
        }
    }
}