using Microsoft.Extensions.Caching.Memory;

namespace Api.Utils
{
    public class CacheHelper : ICacheHelper
    {
        private readonly IMemoryCache _cache;

        public CacheHelper(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGetValue<T>(string key, out List<T> value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public void Set<T>(string key, List<T> value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(expiration ?? TimeSpan.FromMinutes(10));
            _cache.Set(key, value, options);
        }

        public void RemoveKeys(params string[] keys)
        {
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }
    }
}
