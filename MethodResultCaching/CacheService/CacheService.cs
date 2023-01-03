using System.Text.Json;
using StackExchange.Redis;

namespace MethodResultCaching.CacheService
{
    internal class CacheService : ICacheService
    {
        private readonly IDatabase _database;
        private static CacheService? _instance;
        private static readonly object Locker = new();

        private CacheService()
        {
            var config = Environment.GetEnvironmentVariable("redisconnstr");
          
            var redis = ConnectionMultiplexer.Connect(config);
           
            _database = redis.GetDatabase();
        }

        public static CacheService Instance
        {
            get
            {
                if (_instance == null)
                {
                    // thread safety
                    lock (Locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new CacheService();
                        }
                    }
                }

                return _instance;
            }
        }
        public async Task<T?> GetAsync<T>(string cacheKey)
        {
            var result = await _database.StringGetAsync(cacheKey);

            return result == RedisValue.Null ? default : ConvertBytes<T>(result);
        }

        public Task SetAsync(string cacheKey, object value)
        {
            return _database.StringSetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(value));
        }

        private T ConvertBytes<T>(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);

            var result = JsonSerializer.Deserialize<T>(ms);

            if (result == null)
            {
                throw new ApplicationException($"{nameof(result)} cannot be null, check your cache object type");
            }

            return result;
        }
    }
}
