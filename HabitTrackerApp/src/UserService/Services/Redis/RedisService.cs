using StackExchange.Redis;

namespace UserService.Services.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public Task SetAsync(string key, string value, TimeSpan? expiry = null)
            => _db.StringSetAsync(key, value, expiry);

        public async Task<string?> GetAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }

        public Task RemoveAsync(string key)
            => _db.KeyDeleteAsync(key);
    }

}
