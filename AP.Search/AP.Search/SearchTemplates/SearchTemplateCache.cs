using System;
using YM.Elasticsearch;

namespace AP.Search.SearchTemplates
{
    class SearchTemplateCache : SimpleCache<SearchTemplate>
    {
        private readonly Action _cleanup;

        public SearchTemplateCache(Action cleanup, int cleanupInterval = 5 * 60 * 1000) : 
            base(cleanupInterval)
        {
            _cleanup = cleanup;
        }

        public override void Cleanup(object state)
        {
            base.Cleanup(state);
            _cleanup();
        }
    }
}
