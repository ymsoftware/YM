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
        private readonly string _clusterUrl;

        public ElasticsearchClient()
        {
            _clusterUrl = "http://localhost:9200";
        }

        public ElasticsearchClient(Uri clusterUri)
        {
            _clusterUrl = string.Format("{0}://{1}:{2}", clusterUri.Scheme, clusterUri.Host, clusterUri.Port);
        }

        public ElasticsearchClient(string clusterUrl)
        {
            var uri = new Uri(clusterUrl);
            _clusterUrl = string.Format("{0}://{1}:{2}", uri.Scheme, uri.Host, uri.Port);
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(CreateIndexRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Put, request.GetBody());
            return new CreateIndexResponse(response);
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(string index)
        {
            return await CreateIndexAsync(new CreateIndexRequest(index));
        }

        public async Task<DeleteIndexResponse> DeleteIndexAsync(string index)
        {
            string url = string.Format("{0}/{1}", _clusterUrl, index);
            var response = await SendAsync(url, HttpMethod.Delete, null);
            return new DeleteIndexResponse(response);
        }

        public async Task<bool> IndexExistsAsync(string index)
        {
            string url = string.Format("{0}/{1}", _clusterUrl, index);
            var response = await SendAsync(url, HttpMethod.Head, null);
            return response.Http.IsSuccessStatusCode;
        }

        public async Task<AliasResponse> AliasAsync(AliasRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            return new AliasResponse(response);
        }

        public async Task<GetIndexAliasesResponse> GetIndexAliasesAsync(string index)
        {
            string url = string.Format("{0}/{1}/_alias", _clusterUrl, index);
            var response = await SendAsync(url, HttpMethod.Get, null);
            return new GetIndexAliasesResponse(response);
        }

        public async Task<GetAliasIndicesResponse> GetAliasIndicesAsync(string alias)
        {
            string url = string.Format("{0}/_alias/{1}", _clusterUrl, alias);
            var response = await SendAsync(url, HttpMethod.Get, null);
            return new GetAliasIndicesResponse(response);
        }

        public async Task<CatIndexResponse> CatIndexAsync(string index)
        {
            string url = string.Format("{0}/_cat/indices/{1}", _clusterUrl, index);
            var response = await SendAsync(url, HttpMethod.Get, null);
            return new CatIndexResponse(response);
        }

        public async Task<IndexDocumentResponse> IndexDocumentAsync(IndexDocumentRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Put, request.GetBody());
            return new IndexDocumentResponse(response);
        }

        public async Task<IndexDocumentResponse> IndexDocumentAsync(ElasticsearchDocument document)
        {
            return await IndexDocumentAsync(new IndexDocumentRequest(document));
        }

        public async Task<GetDocumentResponse> GetDocumentAsync(GetDocumentRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Get, null);
            return new GetDocumentResponse(response);
        }

        public async Task<DeleteDocumentResponse> DeleteDocumentAsync(DeleteDocumentRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Delete, null);
            return new DeleteDocumentResponse(response);
        }

        public async Task<BulkResponse> BulkDocumentsAsync(BulkRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
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
            var response = await SendAsync(_clusterUrl, HttpMethod.Get, null);
            return new ClusterResponse(JsonObject.Parse(response.Content));
        }

        public async Task<string> SearchAsStringAsync(SearchRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            return response.Content;
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            return new SearchResponse(response);
        }

        public async Task<string> SearchUriAsStringAsync(string searchUri)
        {
            string url = string.Format("{0}{1}{2}", _clusterUrl, searchUri.StartsWith("/") ? "" : "/", searchUri);
            var response = await SendAsync(url, HttpMethod.Get, null);
            return response.Content;
        }

        public async Task<SearchResponse> SearchUriAsync(string searchUri)
        {
            string url = string.Format("{0}{1}{2}", _clusterUrl, searchUri.StartsWith("/") ? "" : "/", searchUri);
            var response = await SendAsync(url, HttpMethod.Get, null);
            return new SearchResponse(response);
        }

        public async Task<ScrollResponse> ScrollAsync(ScrollRequest request)
        {
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            return new ScrollResponse(response);
        }

        public async Task<SearchAfterResponse> SearchAfterAsync(SearchRequest request)
        {
            request.SetFrom(0);
            string url = request.GetUrl(_clusterUrl);
            var response = await SendAsync(url, HttpMethod.Post, request.GetBody());
            return new SearchAfterResponse(response);
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
