namespace CsvFileUploadSaveDb.Helpers
{
    using Microsoft.Extensions.Caching.Distributed;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class CacheHelper : ICacheHelper
    {
        private readonly IDistributedCache _cache;

        public CacheHelper(IDistributedCache cache)
        {
            _cache = cache;
        }

        public string GetString(string key)
        {
            return _cache.GetString(key);
        }

        public T GetValue<T>(string key) where T : class
        {
            string result = _cache.GetString(key);

            if (String.IsNullOrEmpty(result))
            {
                return null;
            }

            var value = JsonSerializer.Deserialize<T>(result);
            return value;
        }

        public void SetString(string key, string value)
        {
            DistributedCacheEntryOptions cacheEntryOptions = GetCacheEntryOptions();
            _cache.SetString(key, value, cacheEntryOptions);
        }

        public void SetValue<T>(string key, T value) where T : class
        {
            DistributedCacheEntryOptions cacheEntryOptions = GetCacheEntryOptions();

            string result = JsonSerializer.Serialize(value);
            _cache.SetString(key, result, cacheEntryOptions);
        }

        private DistributedCacheEntryOptions GetCacheEntryOptions()
        {
            var cacheEntryOptions = new DistributedCacheEntryOptions();

            // Remove item from cache after duration
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);

            // Remove item from cache if unsued for the duration
            cacheEntryOptions.SlidingExpiration = TimeSpan.FromSeconds(30);
            return cacheEntryOptions;
        }
    }
}
