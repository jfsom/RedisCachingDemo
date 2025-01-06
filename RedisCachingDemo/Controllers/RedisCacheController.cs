using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace RedisCachingDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisCacheController : ControllerBase
    {
        // Dependency for interacting with Redis via IDistributedCache
        private readonly IDistributedCache _distributedCache;

        // Dependency for interacting directly with Redis using StackExchange.Redis
        private readonly IConnectionMultiplexer _redisConnection;

        private readonly IConfiguration _configuration;

        // Constructor for injecting dependencies
        public RedisCacheController(IDistributedCache distributedCache, IConnectionMultiplexer redisConnection, IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            _redisConnection = redisConnection;
            _configuration = configuration;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCachedKeysAndValues()
        {
            try
            {
                // Get the Redis server instance from the connection multiplexer
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

                // Retrieve all keys from the Redis database
                var keys = server.Keys().ToArray();

                // Get the InstanceName from the configuration (as specified in appsettings.json)
                string instanceName = _configuration["RedisCacheOptions:InstanceName"] ?? string.Empty;

                // Initialize a list to store key-value pairs from the cache
                var cacheEntries = new List<KeyValuePair<string, string>>();

                // Iterate over each key and retrieve its value
                foreach (var key in keys)
                {
                    // Remove the instance name prefix if it exists
                    var keyWithoutPrefix = key.ToString().Replace($"{instanceName}", "");

                    // Get the value associated with the current key from the distributed cache
                    var value = await _distributedCache.GetStringAsync(keyWithoutPrefix);

                    // Add the key-value pair to the list; if value is null, set it to "null"
                    cacheEntries.Add(new KeyValuePair<string, string>(keyWithoutPrefix, value ?? "null"));
                }

                // Return the list of key-value pairs as a JSON response
                return Ok(cacheEntries);
            }
            catch (Exception ex)
            {
                // Handle any errors and return a 500 Internal Server Error response
                return StatusCode(500, new { message = "Failed to retrieve cache entries.", error = ex.Message });
            }
        }

        // Retrieve a specific cache entry by its key.
        [HttpGet("{key}")]
        public async Task<IActionResult> GetCacheEntryByKey(string key)
        {
            try
            {
                // Attempt to retrieve the value associated with the provided key asynchronously
                var value = await _distributedCache.GetStringAsync(key);

                // If the key is not found in the cache, return a 404 Not Found response
                if (value == null)
                {
                    return NotFound(new { message = "Cache entry not found." });
                }

                // Return the key-value pair as a JSON response
                return Ok(new { Key = key, Value = value });
            }
            catch (Exception ex)
            {
                // Handle any errors and return a 500 Internal Server Error response
                return StatusCode(500, new { message = "Failed to retrieve cache entry.", error = ex.Message });
            }
        }

        // Clear all cache entries from Redis.
        [HttpDelete("all")]
        public IActionResult ClearAllCacheEntries()
        {
            try
            {
                // Get the Redis server instance from the connection multiplexer
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

                // Iterate over all keys in the Redis database
                foreach (var key in server.Keys())
                {
                    // Get the InstanceName from the configuration (as specified in appsettings.json)
                    string instanceName = _configuration["RedisCacheOptions:InstanceName"] ?? string.Empty;

                    // Remove the instance name prefix if it exists
                    var keyWithoutPrefix = key.ToString().Replace($"{instanceName}:", "");

                    // Remove each key-value pair from the distributed cache
                    _distributedCache.Remove(keyWithoutPrefix);
                }

                // Return a success message
                return Ok(new { message = "All cache entries cleared." });
            }
            catch (Exception ex)
            {
                // Handle any errors and return a 500 Internal Server Error response
                return StatusCode(500, new { message = "Failed to clear cache entries.", error = ex.Message });
            }
        }

        // Clear a specific cache entry by its key.
        [HttpDelete("{key}")]
        public async Task<IActionResult> ClearCacheEntryByKey(string key)
        {
            try
            {
                // Remove the cache entry associated with the provided key asynchronously
                await _distributedCache.RemoveAsync(key);

                // Return a success message indicating the specific cache entry was cleared
                return Ok(new { message = $"Cache entry '{key}' cleared." });
            }
            catch (Exception ex)
            {
                // Handle any errors and return a 500 Internal Server Error response
                return StatusCode(500, new { message = "Failed to clear cache entry.", error = ex.Message });
            }
        }
    }
}