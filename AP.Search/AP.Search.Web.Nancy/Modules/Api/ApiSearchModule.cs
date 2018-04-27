using AP.Search.SearchTemplates;
using Nancy;
using System.Collections.Generic;
using System.Threading.Tasks;
using YM.Elasticsearch.Client;
using YM.Elasticsearch.Client.Search;
using YM.Json;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiSearchModule : ApiModuleBase
    {
        public const string API_SEARCH = "/api/search";

        public ApiSearchModule()
        {
            Post(API_SEARCH, async args => await ExecuteAsync());
        }

        protected override async Task<ApiResponse> InternalExecuteAsync(object request)
        {
            var es = new ElasticsearchClient(AppSettings.Current.ClusterUrl);

            SearchRequest sr = null;

            var body = RequestBody();
            var jo = string.IsNullOrWhiteSpace(body) ? null : JsonObject.Parse(body);

            if (jo == null || jo.IsEmpty)
            {
                sr = new SearchRequest();
            }
            else
            {
                sr = await GetSearchRequest(jo);
            }

            var response = await es.SearchAsync(sr);
            return new ApiResponse(response.ToString(), (HttpStatusCode)(int)response.StatusCode);
        }

        private async Task<SearchRequest> GetSearchRequest(JsonObject jo)
        {
            if (jo == null || jo.IsEmpty)
            {
                return new SearchRequest();
            }

            JsonObject temp = new JsonObject();
            IDictionary<string, object> parameters = null;
            SearchTemplate template = null;

            foreach (var jp in jo.Properties())
            {
                switch (jp.Name.ToLower())
                {
                    case "params":
                    case "parameters":
                        parameters = jp.Value.Get<JsonObject>().ToDictionary();
                        break;
                    case "template":
                        if (jp.Value.Type == JsonType.String)
                        {
                            template = await ApiSearchTemplateModules.Repository.GetTemplateAsync(jp.Value.Get<string>());
                        }
                        else if (jp.Value.Type == JsonType.Object)
                        {
                            template = new SearchTemplate(jp.Value.Get<JsonObject>());
                        }
                        break;
                    case "template_id":
                        template = await ApiSearchTemplateModules.Repository.GetTemplateAsync(jp.Value.Get<string>());
                        break;
                    default:
                        temp.Add(jp);
                        break;
                }
            }

            if (template == null)
            {
                if (parameters == null)
                {
                    return new SearchRequest(temp);
                }
                else
                {
                    return new SearchRequest(temp.SetParameters(parameters));
                }
            }
            else
            {
                if (parameters == null)
                {
                    parameters = temp.ToDictionary();
                }
                return template.GetSearchRequest(parameters);
            }
        }
    }
}