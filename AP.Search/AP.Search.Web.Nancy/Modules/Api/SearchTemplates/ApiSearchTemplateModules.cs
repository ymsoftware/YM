using AP.Search.SearchTemplates;
using YM.Elasticsearch;

namespace AP.Search.Web.Nancy.Modules.Api
{
    static class ApiSearchTemplateModules
    {
        public const string API_SEARCH_TEMPLATES = "/api/search/templates";

        private static readonly SearchTemplateRepository _templates = new SearchTemplateRepository(CacheExpiration.Sliding(5 * 60 * 1000));

        public static SearchTemplateRepository Repository
        {
            get
            {
                return _templates;
            }
        }
    }
}