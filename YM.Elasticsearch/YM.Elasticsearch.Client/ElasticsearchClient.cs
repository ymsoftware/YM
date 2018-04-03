using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using YM.Elasticsearch.Client.Cluster;
using YM.Elasticsearch.Client.Documents;
using YM.Elasticsearch.Client.Indices;
using YM.Elasticsearch.Client.Search;
using YM.Json;

namespace YM.Elasticsearch.Client
{
    public class ElasticsearchClient
    {
        public const string APPLICATION_JSON = "application/json";

        private readonly HttpClient _http = new HttpClient();        
        private readonly string _es;

        public ElasticsearchClient()
        {
            _es = "http://localhost:9200";
        }

        public ElasticsearchClient(Uri uri)
        {
            _es = string.Format("{0}://{1}:{2}", uri.Scheme, uri.Host, uri.Port);
        }

        public ElasticsearchClient(string url)
        {
            var uri = new Uri(url);
            _es = string.Format("{0}://{1}:{2}", uri.Scheme, uri.Host, uri.Port);
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(CreateIndexRequest request)
        {
            string url = request.GetUrl(_es);
            var response = await SendAsync(url, HttpMethod.Put, request.GetBody());
            return new CreateIndexResponse(response);
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(string index)
        {
            return await CreateIndexAsync(new CreateIndexRequest(index));
        }

        public async Task<DeleteIndexResponse> DeleteIndexAsync(string index)
        {
            string url = string.Format("{0}/{1}", _es, index);
            var response = await SendAsync(url, HttpMethod.Delete, null);
            return new DeleteIndexResponse(response);
        }

        public async Task<IndexDocumentResponse> IndexDocumentAsync(IndexDocumentRequest request)
        {
            string url = request.GetUrl(_es);
            var response = await SendAsync(url, HttpMethod.Put, request.GetBody());
            return new IndexDocumentResponse(response);
        }

        public async Task<IndexDocumentResponse> IndexDocumentAsync(ElasticsearchDocument document)
        {
            return await IndexDocumentAsync(new IndexDocumentRequest(document));
        }

        public async Task<GetDocumentResponse> GetDocumentAsync(GetDocumentRequest request)
        {
            string url = request.GetUrl(_es);
            var response = await SendAsync(url, HttpMethod.Get, null);
            return new GetDocumentResponse(response);
        }

        public async Task<DeleteDocumentResponse> DeleteDocumentAsync(DeleteDocumentRequest request)
        {
            string url = request.GetUrl(_es);
            var response = await SendAsync(url, HttpMethod.Delete, null);
            return new DeleteDocumentResponse(response);
        }

        public async Task<BulkResponse> BulkDocumentsAsync(BulkRequest request)
        {
            string url = request.GetUrl(_es);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            return new BulkResponse(response);
        }

        public async Task<BulkResponse> BulkIndexDocumentsAsync(IEnumerable<ElasticsearchDocument> documents)
        {
            var request = new BulkRequest().Add(documents.Select(e => new BulkRequestItem("index", e.Id, e.Index, e.Type, e.Source)));
            return await BulkDocumentsAsync(request);
        }

        public async Task<ClusterResponse> GetClusterAsync()
        {
            var response = await SendAsync(_es, HttpMethod.Get, null);
            return new ClusterResponse(JsonObject.Parse(response.Content));
        }

        public async Task<string> SearchAsStringAsync(SearchRequest request)
        {
            string url = request.GetUrl(_es);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            return response.Content;
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            string json = await SearchAsStringAsync(request);
            var jo = JsonObject.Parse(json);
            return new SearchResponse(jo);
        }

        public async Task<ScrollResponse> ScrollAsync(ScrollRequest request)
        {
            string url = request.GetUrl(_es);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            var jo = JsonObject.Parse(response.Content);
            return new ScrollResponse(jo);
        }

        public async Task<SearchAfterResponse> SearchAfterAsync(SearchRequest request)
        {
            request.SetFrom(0);
            string json = await SearchAsStringAsync(request);
            var jo = JsonObject.Parse(json);
            return new SearchAfterResponse(jo);
        }

        private async Task<RestResponse> SendAsync(string url, HttpMethod method, string body)
        {
            var request = new HttpRequestMessage(method, url);
            
            if (!string.IsNullOrWhiteSpace(body) && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                request.Content = new StringContent(body);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(APPLICATION_JSON);
            }

            var http = await _http.SendAsync(request);
            string content = await http.Content.ReadAsStringAsync();

            return new RestResponse(http, content);
        }
    }
}
