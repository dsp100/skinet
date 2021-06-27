using System;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Services
{
    public class ResponsCachedService : IResponseCacheService
    {
        private readonly IDatabase _database;

        
        public ResponsCachedService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        //puts things in database
        public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
        {
            if(response == null)
            {
                return;
            }

            var options = new JsonSerializerOptions
            {
               PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };

            var serialisedResponse = JsonSerializer.Serialize(response, options);

            await _database.StringSetAsync(cacheKey, serialisedResponse, timeToLive);


        }

        //Gets data out of database
        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            var cachedResponse = await _database.StringGetAsync(cacheKey);

            if (cachedResponse.IsNullOrEmpty)
            {
                return null;
            }

            return cachedResponse;
        }
    }
}