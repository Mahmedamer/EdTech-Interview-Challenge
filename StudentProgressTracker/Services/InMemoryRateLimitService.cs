using Microsoft.Extensions.Options;
using StudentProgressTracker.Configuration;
using StudentProgressTracker.Services.Interfaces;
using System.Collections.Concurrent;

namespace StudentProgressTracker.Services
{
    /// <summary>
    /// In-memory implementation of rate limiting service
    /// </summary>
    public class InMemoryRateLimitService : IRateLimitService
    {
        private readonly RateLimitOptions _options;
        private readonly ILogger<InMemoryRateLimitService> _logger;
        private readonly ConcurrentDictionary<string, RateLimitInfo> _clientData = new();
        private readonly ConcurrentDictionary<string, int> _concurrentRequests = new();
        private readonly object _lockObject = new();

        // Statistics tracking
        private int _totalRequests = 0;
        private int _blockedRequests = 0;
        private readonly ConcurrentDictionary<string, int> _requestsByEndpoint = new();
        private readonly ConcurrentDictionary<string, int> _blockedByEndpoint = new();
        private readonly ConcurrentDictionary<string, int> _requestsByClient = new();

        public InMemoryRateLimitService(IOptions<RateLimitOptions> options, ILogger<InMemoryRateLimitService> logger)
        {
            _options = options.Value;
            _logger = logger;

            // Start cleanup task
            _ = Task.Run(CleanupExpiredDataAsync);
        }

        public async Task<RateLimitResult> CheckRateLimitAsync(string clientId, string endpoint, string ipAddress)
        {
            try
            {
                // Check if rate limiting is disabled
                if (!_options.Enabled)
                {
                    return new RateLimitResult { IsAllowed = true };
                }

                // Check whitelists
                if (_options.WhitelistedIPs.Contains(ipAddress) || _options.WhitelistedClients.Contains(clientId))
                {
                    return new RateLimitResult { IsAllowed = true };
                }

                var clientKey = GetClientKey(clientId, ipAddress);
                var now = DateTime.UtcNow;

                lock (_lockObject)
                {
                    var clientInfo = _clientData.GetOrAdd(clientKey, k => new RateLimitInfo
                    {
                        ClientId = clientId,
                        Endpoint = endpoint,
                        WindowStart = now,
                        LastRequest = now,
                        ClientTier = DetermineClientTier(clientId)
                    });

                    // Get applicable rules
                    var rules = GetApplicableRules(endpoint, clientInfo.ClientTier);

                    // Check if client is under penalty
                    if (clientInfo.PenaltyUntil.HasValue && now < clientInfo.PenaltyUntil.Value)
                    {
                        var retryAfter = clientInfo.PenaltyUntil.Value - now;
                        return new RateLimitResult
                        {
                            IsAllowed = false,
                            Reason = "Client is under penalty for previous rate limit violations",
                            RetryAfter = retryAfter,
                            LimitInfo = clientInfo,
                            Headers = CreateHeaders(clientInfo, rules, retryAfter)
                        };
                    }

                    // Reset counters if time windows have passed
                    ResetCountersIfNeeded(clientInfo, now);

                    // Check concurrent requests
                    var concurrentKey = $"concurrent_{clientKey}";
                    var currentConcurrent = _concurrentRequests.GetOrAdd(concurrentKey, 0);
                    
                    if (rules.MaxConcurrentRequests > 0 && currentConcurrent >= rules.MaxConcurrentRequests)
                    {
                        return new RateLimitResult
                        {
                            IsAllowed = false,
                            Reason = "Maximum concurrent requests exceeded",
                            RetryAfter = TimeSpan.FromSeconds(1),
                            LimitInfo = clientInfo,
                            Headers = CreateHeaders(clientInfo, rules, TimeSpan.FromSeconds(1))
                        };
                    }

                    // Check rate limits
                    var rateLimitCheck = CheckRateLimits(clientInfo, rules, now);
                    if (!rateLimitCheck.IsAllowed)
                    {
                        // Apply penalty if enabled
                        if (rules.EnableProgressivePenalties)
                        {
                            ApplyPenalty(clientInfo, rules);
                        }

                        Interlocked.Increment(ref _blockedRequests);
                        _blockedByEndpoint.AddOrUpdate(endpoint, 1, (k, v) => v + 1);

                        return rateLimitCheck;
                    }

                    return new RateLimitResult
                    {
                        IsAllowed = true,
                        LimitInfo = clientInfo,
                        Headers = CreateHeaders(clientInfo, rules, null)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for client {ClientId}", clientId);
                // Fail open - allow request if rate limiting service fails
                return new RateLimitResult { IsAllowed = true };
            }
        }

        public async Task RecordRequestAsync(string clientId, string endpoint, string ipAddress)
        {
            try
            {
                if (!_options.Enabled) return;

                var clientKey = GetClientKey(clientId, ipAddress);
                var concurrentKey = $"concurrent_{clientKey}";
                var now = DateTime.UtcNow;

                lock (_lockObject)
                {
                    var clientInfo = _clientData.GetOrAdd(clientKey, k => new RateLimitInfo
                    {
                        ClientId = clientId,
                        Endpoint = endpoint,
                        WindowStart = now,
                        LastRequest = now,
                        ClientTier = DetermineClientTier(clientId)
                    });

                    // Update counters
                    clientInfo.RequestsInCurrentSecond++;
                    clientInfo.RequestsInCurrentMinute++;
                    clientInfo.RequestsInCurrentHour++;
                    clientInfo.RequestsInCurrentDay++;
                    clientInfo.LastRequest = now;

                    // Update concurrent counter
                    _concurrentRequests.AddOrUpdate(concurrentKey, 1, (k, v) => v + 1);

                    // Schedule concurrent counter decrement (simulate request completion)
                    _ = Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ =>
                    {
                        _concurrentRequests.AddOrUpdate(concurrentKey, 0, (k, v) => Math.Max(0, v - 1));
                    });
                }

                // Update statistics
                Interlocked.Increment(ref _totalRequests);
                _requestsByEndpoint.AddOrUpdate(endpoint, 1, (k, v) => v + 1);
                _requestsByClient.AddOrUpdate(clientId, 1, (k, v) => v + 1);

                _logger.LogDebug("Recorded request for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording request for client {ClientId}", clientId);
            }
        }

        public async Task<RateLimitInfo> GetRateLimitInfoAsync(string clientId)
        {
            var clientKey = GetClientKey(clientId, "unknown");
            return _clientData.TryGetValue(clientKey, out var info) ? info : new RateLimitInfo { ClientId = clientId };
        }

        public async Task ResetRateLimitAsync(string clientId)
        {
            try
            {
                var keysToRemove = _clientData.Keys.Where(k => k.StartsWith($"{clientId}_")).ToList();
                foreach (var key in keysToRemove)
                {
                    _clientData.TryRemove(key, out _);
                }

                _logger.LogInformation("Reset rate limits for client {ClientId}", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting rate limit for client {ClientId}", clientId);
            }
        }

        public async Task<RateLimitStatistics> GetStatisticsAsync()
        {
            return new RateLimitStatistics
            {
                TotalRequests = _totalRequests,
                BlockedRequests = _blockedRequests,
                RequestsByEndpoint = new Dictionary<string, int>(_requestsByEndpoint),
                BlockedByEndpoint = new Dictionary<string, int>(_blockedByEndpoint),
                RequestsByClient = new Dictionary<string, int>(_requestsByClient),
                TopClients = _requestsByClient
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(10)
                    .Select(kvp => $"{kvp.Key} ({kvp.Value} requests)")
                    .ToList(),
                TopEndpoints = _requestsByEndpoint
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(10)
                    .Select(kvp => $"{kvp.Key} ({kvp.Value} requests)")
                    .ToList()
            };
        }

        #region Private Methods

        private string GetClientKey(string clientId, string ipAddress)
        {
            return string.IsNullOrEmpty(clientId) ? $"ip_{ipAddress}" : $"{clientId}_{ipAddress}";
        }

        private string DetermineClientTier(string clientId)
        {
            // Simple tier determination logic - in real scenarios, this would query a database
            if (clientId.StartsWith("premium_")) return "Premium";
            if (clientId.StartsWith("standard_")) return "Standard";
            return "Basic";
        }

        private RateLimitRule GetApplicableRules(string endpoint, string clientTier)
        {
            // Check endpoint-specific rules first
            if (_options.EndpointRules.TryGetValue(endpoint, out var endpointRule))
            {
                return endpointRule;
            }

            // Check client tier rules
            if (_options.ClientTiers.TryGetValue(clientTier, out var tierRule))
            {
                return tierRule;
            }

            // Fall back to general rules
            return _options.GeneralRules;
        }

        private void ResetCountersIfNeeded(RateLimitInfo clientInfo, DateTime now)
        {
            // Reset second counter
            if ((now - clientInfo.LastRequest).TotalSeconds >= 1)
            {
                clientInfo.RequestsInCurrentSecond = 0;
            }

            // Reset minute counter
            if ((now - clientInfo.WindowStart).TotalMinutes >= 1)
            {
                clientInfo.RequestsInCurrentMinute = 0;
                clientInfo.WindowStart = now;
            }

            // Reset hour counter
            if ((now - clientInfo.WindowStart).TotalHours >= 1)
            {
                clientInfo.RequestsInCurrentHour = 0;
            }

            // Reset day counter
            if ((now - clientInfo.WindowStart).TotalDays >= 1)
            {
                clientInfo.RequestsInCurrentDay = 0;
            }
        }

        private RateLimitResult CheckRateLimits(RateLimitInfo clientInfo, RateLimitRule rules, DateTime now)
        {
            // Check per-second limit
            if (rules.RequestsPerSecond > 0 && clientInfo.RequestsInCurrentSecond >= rules.RequestsPerSecond)
            {
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Reason = "Per-second rate limit exceeded",
                    RetryAfter = TimeSpan.FromSeconds(1),
                    LimitInfo = clientInfo,
                    Headers = CreateHeaders(clientInfo, rules, TimeSpan.FromSeconds(1))
                };
            }

            // Check per-minute limit
            if (rules.RequestsPerMinute > 0 && clientInfo.RequestsInCurrentMinute >= rules.RequestsPerMinute)
            {
                var retryAfter = TimeSpan.FromSeconds(60 - (now - clientInfo.WindowStart).TotalSeconds);
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Reason = "Per-minute rate limit exceeded",
                    RetryAfter = retryAfter,
                    LimitInfo = clientInfo,
                    Headers = CreateHeaders(clientInfo, rules, retryAfter)
                };
            }

            // Check per-hour limit
            if (rules.RequestsPerHour > 0 && clientInfo.RequestsInCurrentHour >= rules.RequestsPerHour)
            {
                var retryAfter = TimeSpan.FromMinutes(60 - (now - clientInfo.WindowStart).TotalMinutes);
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Reason = "Per-hour rate limit exceeded",
                    RetryAfter = retryAfter,
                    LimitInfo = clientInfo,
                    Headers = CreateHeaders(clientInfo, rules, retryAfter)
                };
            }

            // Check per-day limit
            if (rules.RequestsPerDay > 0 && clientInfo.RequestsInCurrentDay >= rules.RequestsPerDay)
            {
                var retryAfter = TimeSpan.FromHours(24 - (now - clientInfo.WindowStart).TotalHours);
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Reason = "Per-day rate limit exceeded",
                    RetryAfter = retryAfter,
                    LimitInfo = clientInfo,
                    Headers = CreateHeaders(clientInfo, rules, retryAfter)
                };
            }

            return new RateLimitResult { IsAllowed = true };
        }

        private void ApplyPenalty(RateLimitInfo clientInfo, RateLimitRule rules)
        {
            clientInfo.ViolationCount++;
            var penaltyDuration = TimeSpan.FromMinutes(Math.Pow(rules.PenaltyMultiplier, clientInfo.ViolationCount));
            clientInfo.PenaltyUntil = DateTime.UtcNow.Add(penaltyDuration);

            _logger.LogWarning("Applied penalty to client {ClientId}. Violation count: {ViolationCount}, Penalty until: {PenaltyUntil}",
                clientInfo.ClientId, clientInfo.ViolationCount, clientInfo.PenaltyUntil);
        }

        private Dictionary<string, string> CreateHeaders(RateLimitInfo clientInfo, RateLimitRule rules, TimeSpan? retryAfter)
        {
            var headers = new Dictionary<string, string>();

            if (_options.ResponseConfig.IncludeHeaders)
            {
                headers["X-RateLimit-Limit-Minute"] = rules.RequestsPerMinute.ToString();
                headers["X-RateLimit-Limit-Hour"] = rules.RequestsPerHour.ToString();
                headers["X-RateLimit-Limit-Day"] = rules.RequestsPerDay.ToString();
                headers["X-RateLimit-Remaining-Minute"] = Math.Max(0, rules.RequestsPerMinute - clientInfo.RequestsInCurrentMinute).ToString();
                headers["X-RateLimit-Remaining-Hour"] = Math.Max(0, rules.RequestsPerHour - clientInfo.RequestsInCurrentHour).ToString();
                headers["X-RateLimit-Remaining-Day"] = Math.Max(0, rules.RequestsPerDay - clientInfo.RequestsInCurrentDay).ToString();
                headers["X-RateLimit-Reset"] = ((DateTimeOffset)clientInfo.WindowStart.AddMinutes(1)).ToUnixTimeSeconds().ToString();
                headers["X-RateLimit-Client-Tier"] = clientInfo.ClientTier;
            }

            if (_options.ResponseConfig.IncludeRetryAfter && retryAfter.HasValue)
            {
                headers["Retry-After"] = ((int)retryAfter.Value.TotalSeconds).ToString();
            }

            // Add custom headers
            foreach (var customHeader in _options.ResponseConfig.CustomHeaders)
            {
                headers[customHeader.Key] = customHeader.Value;
            }

            return headers;
        }

        private async Task CleanupExpiredDataAsync()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5)); // Cleanup every 5 minutes

                    var now = DateTime.UtcNow;
                    var keysToRemove = new List<string>();

                    foreach (var kvp in _clientData)
                    {
                        if ((now - kvp.Value.LastRequest) > _options.StorageConfig.DataRetention)
                        {
                            keysToRemove.Add(kvp.Key);
                        }
                    }

                    foreach (var key in keysToRemove)
                    {
                        _clientData.TryRemove(key, out _);
                    }

                    if (keysToRemove.Count > 0)
                    {
                        _logger.LogDebug("Cleaned up {Count} expired rate limit entries", keysToRemove.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during rate limit cleanup");
                }
            }
        }

        #endregion
    }
}