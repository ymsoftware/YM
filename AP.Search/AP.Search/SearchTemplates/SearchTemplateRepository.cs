using System;
using System.Threading.Tasks;
using YM.Elasticsearch;
using YM.Elasticsearch.Client;
using YM.Elasticsearch.Client.Documents;
using YM.Elasticsearch.Client.Indices;
using YM.Elasticsearch.Client.Search;
using YM.Json;

namespace AP.Search.SearchTemplates
{
    public class SearchTemplateRepository
    {
        public const string DEFAULT_CLUSTER = "http://localhost:9200";
        public const string DEFAULT_INDEX = "search_templates";

        private readonly ElasticsearchClient _es;
        private readonly string _index;
        private readonly ISimpleCache<SearchTemplate> _cache;

        private CacheExpiration _expiration = CacheExpiration.Sliding(5 * 60 * 1000);

        public SearchTemplateRepository(string clusterUrl, string index, ISimpleCache<SearchTemplate> cache = null)
        {
            _es = new ElasticsearchClient(clusterUrl);
            _index = index;
            _cache = cache;
        }

        public SearchTemplateRepository(ISimpleCache<SearchTemplate> cache = null)
        {
            _es = new ElasticsearchClient(DEFAULT_CLUSTER);
            _index = DEFAULT_INDEX;
            _cache = cache;
        }

        public async Task<SearchTemplate> GetTemplateAsync(string id)
        {
            SearchTemplate template = null;

            if (_cache != null)
            {
                template = _cache.Get(id);

                if (template == null)
                {
                    template = await GetTemplateFromRepositoryAsync(id);

                    if (template != null)
                    {
                        _cache.Set(id, template, _expiration);
                    }
                }
            }

            return template;
        }

        public async Task<DeleteDocumentResponse> DeleteTemplateAsync(string id, bool refresh = false)
        {
            if (_cache != null)
            {
                _cache.Remove(id);
            }

            var response = await _es.DeleteDocumentAsync(new DeleteDocumentRequest(id, _index, "doc", refresh));
            return response;
        }

        public async Task<IndexDocumentResponse> SaveTemplateAsync(SearchTemplate template, bool refresh = false, CacheExpiration expiration = null)
        {
            if (expiration != null)
            {
                _expiration = expiration;
            }

            if (_cache != null)
            {
                _cache.Set(template.Id, template, _expiration);
            }

            bool exists = await _es.IndexExistsAsync(_index);
            if (!exists)
            {
                var index = await _es.CreateIndexAsync(new CreateIndexRequest(_index, Schema));
                if (!index.IsSuccess)
                {
                    throw new Exception(index.ToString());
                }
            }

            var doc = new ElasticsearchDocument(template.Id, template.ToJson(true), _index);
            var response = await _es.IndexDocumentAsync(new IndexDocumentRequest(doc, refresh));
            return response;
        }

        public async Task<SearchResponse> SearchTemplatesAsync(string query)
        {
            string searchUri = string.Format("/{0}/_search{1}{2}", _index, string.IsNullOrWhiteSpace(query) ? "" : "?", query);
            var response = await _es.SearchUriAsync(searchUri);
            return response;
        }

        public JsonObject Schema
        {
            get
            {
                return new JsonObject()
                    .Add("settings", new JsonObject()
                        .Add("index", new JsonObject()
                            .Add("number_of_shards", 1)
                            .Add("number_of_replicas", 0)))
                    .Add("mappings", new JsonObject()
                        .Add("doc", new JsonObject()
                            .Add("properties", new JsonObject()
                                .Add("id", new JsonObject()
                                    .Add("type", "keyword"))
                                .Add("name", new JsonObject()
                                    .Add("type", "text"))
                                .Add("description", new JsonObject()
                                    .Add("type", "text"))
                                .Add("index", new JsonObject()
                                    .Add("type", "keyword"))
                                .Add("type", new JsonObject()
                                    .Add("type", "keyword"))
                                .Add("source", new JsonObject()
                                    .Add("type", "text")))));
            }
        }

        private async Task<SearchTemplate> GetTemplateFromRepositoryAsync(string id)
        {
            var response = await _es.GetDocumentAsync(new GetDocumentRequest(id, _index));
            return response.IsSuccess ? new SearchTemplate(response.Document.Source) : null;
        }
    }
}