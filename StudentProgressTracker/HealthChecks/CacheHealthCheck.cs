using Microsoft.Extensions.Diagnostics.HealthChecks;
using StudentProgressTracker.Services.Interfaces;

namespace StudentProgressTracker.HealthChecks
{
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheHealthCheck> _logger;

        public CacheHealthCheck(ICacheService cacheService, ILogger<CacheHealthCheck> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var healthCheckKey = "health_check_" + Guid.NewGuid().ToString("N")[..8];
                var testValue = new { Status = "OK", Timestamp = DateTime.UtcNow };

                // Test write operation
                await _cacheService.SetAsync(healthCheckKey, testValue, TimeSpan.FromMinutes(1));

                // Test read operation
                var retrievedValue = await _cacheService.GetAsync<object>(healthCheckKey);

                // Clean up
                await _cacheService.RemoveAsync(healthCheckKey);

                if (retrievedValue != null)
                {
                    // Check if we're using Redis or memory cache by looking at the cache service type
                    var cacheType = _cacheService.GetType().Name;
                    var cacheProvider = cacheType switch
                    {
                        "HybridCacheService" => "Hybrid (Redis + Memory)",
                        "RedisCacheService" => "Redis",
                        "InMemoryCacheService" => "Memory",
                        _ => "Unknown"
                    };
                    
                    return HealthCheckResult.Healthy($"Cache is working properly using {cacheProvider}");
                }
                else
                {
                    return HealthCheckResult.Degraded("Cache write succeeded but read failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache health check failed");
                return HealthCheckResult.Unhealthy("Cache is not accessible", ex);
            }
        }
    }
}
