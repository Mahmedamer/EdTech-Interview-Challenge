using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using StudentProgressTracker.Services.Interfaces;
using System.Text.Json;

namespace StudentProgressTracker.Services
{
    public class HybridCacheService : ICacheService
    {
        private readonly IDistributedCache? _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<HybridCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _redisAvailable = true;

        public HybridCacheService(
            IServiceProvider serviceProvider, 
            IMemoryCache memoryCache, 
            ILogger<HybridCacheService> logger)
        {
            _distributedCache = serviceProvider.GetService<IDistributedCache>();
            _memoryCache = memoryCache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            // Test Redis connection on startup
            TestRedisConnection();
        }

        private async void TestRedisConnection()
        {
            if (_distributedCache == null)
            {
                _redisAvailable = false;
                _logger.LogInformation("Redis distributed cache not configured, using memory cache only");
                return;
            }

            try
            {
                var testKey = "connection_test_" + Guid.NewGuid().ToString("N")[..8];
                await _distributedCache.SetStringAsync(testKey, "test");
                await _distributedCache.RemoveAsync(testKey);
                _redisAvailable = true;
                _logger.LogInformation("Redis cache connection verified successfully");
            }
            catch (Exception ex)
            {
                _redisAvailable = false;
                _logger.LogWarning(ex, "Redis cache connection failed, falling back to memory cache");
            }
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            // Try Redis first if available
            if (_redisAvailable && _distributedCache != null)
            {
                try
                {
                    var cachedValue = await _distributedCache.GetStringAsync(key);
                    if (!string.IsNullOrEmpty(cachedValue))
                    {
                        _logger.LogDebug("Cache hit (Redis) for key: {Key}", key);
                        return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis cache read failed for key {Key}, falling back to memory cache", key);
                    _redisAvailable = false;
                }
            }

            // Fallback to memory cache
            try
            {
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    _logger.LogDebug("Cache hit (Memory) for key: {Key}", key);
                    return value as T;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory cache read failed for key {Key}", key);
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return null;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var exp = expiration ?? TimeSpan.FromHours(1);

            // Try Redis first if available
            if (_redisAvailable && _distributedCache != null)
            {
                try
                {
                    var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = exp
                    };

                    await _distributedCache.SetStringAsync(key, serializedValue, options);
                    _logger.LogDebug("Cached value set (Redis) for key: {Key} with expiration: {Expiration}", key, exp);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis cache write failed for key {Key}, falling back to memory cache", key);
                    _redisAvailable = false;
                }
            }

            // Always set in memory cache as backup
            try
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = exp
                };

                _memoryCache.Set(key, value, options);
                _logger.LogDebug("Cached value set (Memory) for key: {Key} with expiration: {Expiration}", key, exp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory cache write failed for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            // Remove from Redis if available
            if (_redisAvailable && _distributedCache != null)
            {
                try
                {
                    await _distributedCache.RemoveAsync(key);
                    _logger.LogDebug("Removed cached value (Redis) for key: {Key}", key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis cache remove failed for key {Key}", key);
                    _redisAvailable = false;
                }
            }

            // Remove from memory cache
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("Removed cached value (Memory) for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory cache remove failed for key {Key}", key);
            }
        }

        public async Task RemovePatternAsync(string pattern)
        {
            try
            {
                // Note: Pattern-based removal is complex for distributed cache
                // For now, we'll log a warning and suggest using specific key removal
                _logger.LogWarning("Pattern-based cache removal not fully implemented for hybrid cache. " +
                    "Consider using specific key removal or implementing a custom solution. Pattern: {Pattern}", pattern);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached values with pattern {Pattern}", pattern);
            }
        }
    }
}
