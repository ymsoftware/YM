using Nancy;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiHomeModule : NancyModule
    {
        public ApiHomeModule()
        {
            Get("/api", _ => AppSettings.Current.ToString());
        }
    }
}
