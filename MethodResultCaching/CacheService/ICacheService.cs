namespace MethodResultCaching.CacheService
{
    internal interface ICacheService
    {
        Task<T?> GetAsync<T>(string cacheKey);

        Task SetAsync(string cacheKey, object value);
    }
}
