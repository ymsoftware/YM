namespace YM.Elasticsearch
{
    public class CacheExpiration
    {
        public ExpirationType Type { get; private set; }
        public int Milliseconds { get; private set; }

        public CacheExpiration(ExpirationType type, int milliseconds)
        {
            Type = type;
            Milliseconds = milliseconds;
        }

        public static CacheExpiration Absolute(int milliseconds)
        {
            return new CacheExpiration(ExpirationType.Absolute, milliseconds);
        }

        public static CacheExpiration Sliding(int milliseconds)
        {
            return new CacheExpiration(ExpirationType.Sliding, milliseconds);
        }
    }
}
