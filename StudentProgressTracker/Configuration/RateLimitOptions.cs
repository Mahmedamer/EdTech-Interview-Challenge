using System.Net;

namespace StudentProgressTracker.Configuration
{
    /// <summary>
    /// Configuration options for API rate limiting
    /// </summary>
    public class RateLimitOptions
    {
        public const string SectionName = "RateLimit";

        /// <summary>
        /// Enable or disable rate limiting globally
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// General rate limit configuration that applies to all endpoints by default
        /// </summary>
        public RateLimitRule GeneralRules { get; set; } = new();

        /// <summary>
        /// Endpoint-specific rate limit configurations
        /// </summary>
        public Dictionary<string, RateLimitRule> EndpointRules { get; set; } = new();

        /// <summary>
        /// Client-specific rate limit configurations based on client identifier
        /// </summary>
        public Dictionary<string, RateLimitRule> ClientRules { get; set; } = new();

        /// <summary>
        /// IP whitelist - IPs that are exempt from rate limiting
        /// </summary>
        public List<string> WhitelistedIPs { get; set; } = new();

        /// <summary>
        /// Client identifier whitelist - clients exempt from rate limiting
        /// </summary>
        public List<string> WhitelistedClients { get; set; } = new();

        /// <summary>
        /// Configuration for different client tiers (Premium, Standard, Basic)
        /// </summary>
        public Dictionary<string, RateLimitRule> ClientTiers { get; set; } = new()
        {
            ["Premium"] = new RateLimitRule { RequestsPerMinute = 1000, RequestsPerHour = 50000, RequestsPerDay = 1000000 },
            ["Standard"] = new RateLimitRule { RequestsPerMinute = 300, RequestsPerHour = 15000, RequestsPerDay = 300000 },
            ["Basic"] = new RateLimitRule { RequestsPerMinute = 100, RequestsPerHour = 5000, RequestsPerDay = 100000 }
        };

        /// <summary>
        /// Response configuration when rate limit is exceeded
        /// </summary>
        public RateLimitResponse ResponseConfig { get; set; } = new();

        /// <summary>
        /// Storage configuration for rate limit data
        /// </summary>
        public RateLimitStorage StorageConfig { get; set; } = new();
    }

    /// <summary>
    /// Rate limiting rules with different time windows
    /// </summary>
    public class RateLimitRule
    {
        /// <summary>
        /// Maximum requests per minute (0 = no limit)
        /// </summary>
        public int RequestsPerMinute { get; set; } = 60;

        /// <summary>
        /// Maximum requests per hour (0 = no limit)
        /// </summary>
        public int RequestsPerHour { get; set; } = 3000;

        /// <summary>
        /// Maximum requests per day (0 = no limit)
        /// </summary>
        public int RequestsPerDay { get; set; } = 50000;

        /// <summary>
        /// Maximum requests per second (0 = no limit) - for burst protection
        /// </summary>
        public int RequestsPerSecond { get; set; } = 10;

        /// <summary>
        /// Maximum concurrent requests per client (0 = no limit)
        /// </summary>
        public int MaxConcurrentRequests { get; set; } = 5;

        /// <summary>
        /// Penalty multiplier when client exceeds limits (increases rate limit duration)
        /// </summary>
        public double PenaltyMultiplier { get; set; } = 1.5;

        /// <summary>
        /// Whether to enable progressive penalties for repeated violations
        /// </summary>
        public bool EnableProgressivePenalties { get; set; } = true;
    }

    /// <summary>
    /// Configuration for rate limit response behavior
    /// </summary>
    public class RateLimitResponse
    {
        /// <summary>
        /// HTTP status code to return when rate limit is exceeded
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.TooManyRequests;

        /// <summary>
        /// Content type for rate limit response
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// Message to include in rate limit response
        /// </summary>
        public string Message { get; set; } = "API rate limit exceeded. Please reduce your request frequency.";

        /// <summary>
        /// Whether to include detailed rate limit information in response headers
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;

        /// <summary>
        /// Whether to include retry-after header
        /// </summary>
        public bool IncludeRetryAfter { get; set; } = true;

        /// <summary>
        /// Custom headers to include in the response
        /// </summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }

    /// <summary>
    /// Storage configuration for rate limiting data
    /// </summary>
    public class RateLimitStorage
    {
        /// <summary>
        /// Storage type: InMemory, Redis, Database
        /// </summary>
        public string Type { get; set; } = "InMemory";

        /// <summary>
        /// Connection string for external storage (Redis, Database)
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Prefix for rate limit keys in storage
        /// </summary>
        public string KeyPrefix { get; set; } = "ratelimit";

        /// <summary>
        /// How long to keep rate limit data in storage (cleanup period)
        /// </summary>
        public TimeSpan DataRetention { get; set; } = TimeSpan.FromDays(1);
    }

    /// <summary>
    /// Rate limit metadata for tracking
    /// </summary>
    public class RateLimitInfo
    {
        public string ClientId { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int RequestsInCurrentMinute { get; set; }
        public int RequestsInCurrentHour { get; set; }
        public int RequestsInCurrentDay { get; set; }
        public int RequestsInCurrentSecond { get; set; }
        public int ConcurrentRequests { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime LastRequest { get; set; }
        public int ViolationCount { get; set; }
        public DateTime? PenaltyUntil { get; set; }
        public string ClientTier { get; set; } = "Basic";
    }
}