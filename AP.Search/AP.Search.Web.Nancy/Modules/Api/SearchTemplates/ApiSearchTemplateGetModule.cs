using Nancy;
using System.Threading.Tasks;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiSearchTemplateGetModule : ApiModuleBase
    {
        public ApiSearchTemplateGetModule()
        {
            Get(ApiSearchTemplateModules.API_SEARCH_TEMPLATES + "/{id}", async args => await ExecuteAsync(args.id));
        }

        protected override async Task<ApiResponse> InternalExecuteAsync(object request)
        {
            var template = await ApiSearchTemplateModules.Repository.GetTemplateAsync(request.ToString());
            return new ApiResponse(template.ToString(), HttpStatusCode.OK);
        }
    }
}