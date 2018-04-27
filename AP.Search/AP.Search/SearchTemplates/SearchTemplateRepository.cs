using System;
using System.Collections.Generic;
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
        private readonly CacheExpiration _expiration;
        private readonly IDictionary<string, int> _versions = new Dictionary<string, int>();        

        public SearchTemplateRepository(string clusterUrl, string index, ISimpleCache<SearchTemplate> cache, CacheExpiration expiration)
        {
            _es = new ElasticsearchClient(clusterUrl);
            _index = index;
            _cache = cache;
            _expiration = expiration;
        }

        public SearchTemplateRepository(CacheExpiration expiration)
        {
            _es = new ElasticsearchClient(DEFAULT_CLUSTER);
            _index = DEFAULT_INDEX;
            _cache = new SearchTemplateCache(CleanupFireAndForget);
            _expiration = expiration;
        }

        public async Task<SearchTemplate> GetTemplateAsync(string id)
        {
            SearchTemplate template = null;

            if (_cache == null)
            {
                template = await GetTemplateFromRepositoryAsync(id);
            }
            else
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

            _versions.Remove(id);

            var response = await _es.DeleteDocumentAsync(new DeleteDocumentRequest(id, _index, "doc", refresh));
            return response;
        }

        public async Task<IndexDocumentResponse> SaveTemplateAsync(SearchTemplate template, bool refresh = false)
        {
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
            string searchUri = string.Format("/{0}/_search{1}{2}", _index, string.IsNullOrWhiteSpace(query) || query.StartsWith("?") ? "" : "?", query);
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
            if (response.IsSuccess)
            {
                _versions[response.Document.Id] = response.Document.Version;
                return new SearchTemplate(response.Document.Source);
            }

            return null;
        }

        private async Task CleanupAsync()
        {
            var cache = _cache as SearchTemplateCache;
            var keys = cache.GetAllKeys();
            if (keys.Length == 0)
            {
                return;
            }

            string searchUri = string.Format("/{0}/_search?version=true&_source=false", _index);
            var response = await _es.SearchUriAsync(searchUri);
            if (response.IsEmpty)
            {
                return;
            }

            foreach (var hit in response.Hits.Hits)
            {
                string key = hit.Id;
                int version = hit.Version;

                if (_versions.TryGetValue(key, out int v) && v < version)
                {
                    _versions.Remove(key);
                    cache.Remove(key);
                }
            }
        }

        private void CleanupFireAndForget()
        {
            Task.Run(() => CleanupAsync());
        }
    }
}