using AP.Search.SearchTemplates;
using Nancy;
using System.Threading.Tasks;
using YM.Json;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiSearchTemplateSaveModule : ApiModuleBase
    {
        public ApiSearchTemplateSaveModule()
        {
            Put(ApiSearchTemplateModules.API_SEARCH_TEMPLATES + "/{id}", async args => await ExecuteAsync(args.id));
            Post(ApiSearchTemplateModules.API_SEARCH_TEMPLATES, async args => await ExecuteAsync());
        }

        protected override async Task<ApiResponse> InternalExecuteAsync(object request)
        {            
            var jo = JsonObject.Parse(RequestBody());

            if (request != null)
            {
                jo
                    .Remove(SearchTemplate.ID)
                    .Add(SearchTemplate.ID, request.ToString());
            }

            var template = new SearchTemplate(jo);
            var response = await ApiSearchTemplateModules.Repository.SaveTemplateAsync(template);
            return new ApiResponse(response.ToString(), (HttpStatusCode)(int)response.StatusCode);
        }
    }
}