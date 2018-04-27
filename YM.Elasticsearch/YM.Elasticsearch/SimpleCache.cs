using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace YM.Elasticsearch
{
    public class SimpleCache<T> : ISimpleCache<T>, IDisposable
    {
        private readonly static object _lock = new object();

        private readonly IDictionary<string, SimpleCacheItem<T>> _items = new Dictionary<string, SimpleCacheItem<T>>();
        private readonly Timer _timer;

        public SimpleCache(int cleanupInterval)
        {
            _timer = new Timer(Cleanup, null, Timeout.Infinite, cleanupInterval);
            _timer.Change(0, cleanupInterval);
            
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public T Get(string key)
        {
            if (_items.TryGetValue(key, out SimpleCacheItem<T> value) && value.Expires > DateTime.Now)
            {
                if (value.Expiration.Type == ExpirationType.Sliding)
                {
                    _items[key] = new SimpleCacheItem<T>(value.Item, value.Expiration);
                }

                return value.Item;
            }

            return default(T);
        }

        public void Remove(string key)
        {
            _items.Remove(key);
        }

        public void Set(string key, T value, CacheExpiration expiration)
        {
            _items[key] = new SimpleCacheItem<T>(value, expiration);
        }

        public string[] GetAllKeys()
        {
            return _items.Keys.ToArray();
        }

        public virtual void Cleanup(object state)
        {
            var dt = DateTime.Now;

            var keys = _items.Keys.ToArray();
            if (keys.Length == 0)
            {
                return;
            }

            lock (_lock)
            {
                foreach (string key in keys)
                {
                    if (_items.TryGetValue(key, out SimpleCacheItem<T> value) && value.Expires < dt)
                    {
                        _items.Remove(key);
                    }
                }
            }
        }
    }
}