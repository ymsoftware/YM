using System;

namespace YM.Elasticsearch
{
    public class SimpleCacheItem<T>
    {
        public T Item { get; private set; }
        public CacheExpiration Expiration { get; private set; }
        public DateTime Expires { get; private set; }

        public SimpleCacheItem(T item, CacheExpiration expiration)
        {
            Item = item;
            Expiration = expiration;
            Expires = DateTime.Now.Add(TimeSpan.FromMilliseconds(expiration.Milliseconds));
        }
    }
}
