using AP.Search.SearchTemplates;
using System;
using System.Runtime.Caching;
using YM.Elasticsearch;

namespace AP.Search.Tests
{
    class SearchTemplateCache : ISimpleCache<SearchTemplate>
    {
        private readonly MemoryCache _cache = MemoryCache.Default;

        public SearchTemplate Get(string key)
        {
            return (SearchTemplate)_cache.Get(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Set(string key, SearchTemplate value, CacheExpiration expiration)
        {
            var policy = new CacheItemPolicy();
            if (expiration.Type == ExpirationType.Sliding)
            {
                policy.SlidingExpiration = TimeSpan.FromMilliseconds(expiration.Milliseconds);
            }
            else 
            {
                policy.AbsoluteExpiration = new DateTimeOffset().AddMilliseconds(expiration.Milliseconds);
            }

            _cache.Set(key, value, policy);
        }
    }
}
