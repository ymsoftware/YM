using Nancy;
using System.Threading.Tasks;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiSearchTemplateDeleteModule : ApiModuleBase
    {
        public ApiSearchTemplateDeleteModule()
        {
            Delete(ApiSearchTemplateModules.API_SEARCH_TEMPLATES + "/{id}", async args => await ExecuteAsync(args.id));
        }

        protected override async Task<ApiResponse> InternalExecuteAsync(object request)
        {
            var response = await ApiSearchTemplateModules.Repository.DeleteTemplateAsync(request.ToString());
            return new ApiResponse(response.ToString(), (HttpStatusCode)(int)response.StatusCode);
        }
    }
}