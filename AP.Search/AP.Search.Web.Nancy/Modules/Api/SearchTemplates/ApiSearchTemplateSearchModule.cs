using Nancy;
using System.Threading.Tasks;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiSearchTemplateSearchModule : ApiModuleBase
    {
        public ApiSearchTemplateSearchModule()
        {
            Get(ApiSearchTemplateModules.API_SEARCH_TEMPLATES + "/search", async args => await ExecuteAsync());
        }

        protected override async Task<ApiResponse> InternalExecuteAsync(object request)
        {
            var response = await ApiSearchTemplateModules.Repository.SearchTemplatesAsync(Request.Url.Query);
            return new ApiResponse(response.ToString(), (HttpStatusCode)(int)response.StatusCode);
        }
    }
}