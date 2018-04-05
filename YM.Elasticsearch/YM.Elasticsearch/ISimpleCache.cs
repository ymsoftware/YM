namespace YM.Elasticsearch
{
    public interface ISimpleCache<T>
    {
        T Get(string key);
        void Remove(string key);
        void Set(string key, T value, CacheExpiration expiration);
    }
}
