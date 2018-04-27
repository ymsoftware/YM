using Nancy;
using System;
using System.Threading.Tasks;
using YM.Elasticsearch.Client;
using YM.Elasticsearch.Client.Search;
using YM.Json;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiSearchDirectModule : ApiModuleBase
    {
        public const string API_SEARCH_ELASTICSEARCH_DIRECT = "/api/search/es/{url*}";

        public ApiSearchDirectModule()
        {
            Get(API_SEARCH_ELASTICSEARCH_DIRECT, async args => await ExecuteAsync(args.url));
            Post(API_SEARCH_ELASTICSEARCH_DIRECT, async args => await ExecuteAsync(args.url));
        }

        protected override async Task<ApiResponse> InternalExecuteAsync(object request)
        {
            string url = string.Format("{0}{1}", request, Request.Url.Query);
            var es = new ElasticsearchClient(AppSettings.Current.ClusterUrl);

            JsonObject jo = null;

            switch (Request.Method)
            {
                case "GET":                    
                    break;
                case "POST":
                    var body = RequestBody();
                    jo = string.IsNullOrWhiteSpace(body) ? null : JsonObject.Parse(body);                    
                    break;
                default:
                    throw new Exception(string.Format("Invalid HTTP methos [{0}]", Request.Method));
            }

            var response = await es.SearchAsync(new SearchRequest(url, jo));
            return new ApiResponse(response.ToString(), (HttpStatusCode)(int)response.StatusCode);
        }
    }
}