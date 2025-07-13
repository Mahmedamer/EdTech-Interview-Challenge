using StudentProgressTracker.Configuration;

namespace StudentProgressTracker.Services.Interfaces
{
    /// <summary>
    /// Interface for rate limiting services
    /// </summary>
    public interface IRateLimitService
    {
        /// <summary>
        /// Check if a request should be allowed based on rate limiting rules
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="endpoint">API endpoint being accessed</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <returns>Rate limit result indicating if request is allowed</returns>
        Task<RateLimitResult> CheckRateLimitAsync(string clientId, string endpoint, string ipAddress);

        /// <summary>
        /// Record a request for rate limiting tracking
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="endpoint">API endpoint being accessed</param>
        /// <param name="ipAddress">Client IP address</param>
        Task RecordRequestAsync(string clientId, string endpoint, string ipAddress);

        /// <summary>
        /// Get current rate limit status for a client
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <returns>Current rate limit information</returns>
        Task<RateLimitInfo> GetRateLimitInfoAsync(string clientId);

        /// <summary>
        /// Reset rate limit counters for a client (admin function)
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        Task ResetRateLimitAsync(string clientId);

        /// <summary>
        /// Get rate limiting statistics
        /// </summary>
        /// <returns>Rate limiting statistics</returns>
        Task<RateLimitStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Rate limit check result
    /// </summary>
    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public string Reason { get; set; } = string.Empty;
        public TimeSpan? RetryAfter { get; set; }
        public RateLimitInfo LimitInfo { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
    }

    /// <summary>
    /// Rate limiting statistics
    /// </summary>
    public class RateLimitStatistics
    {
        public int TotalRequests { get; set; }
        public int BlockedRequests { get; set; }
        public double BlockedPercentage => TotalRequests > 0 ? (BlockedRequests * 100.0 / TotalRequests) : 0;
        public Dictionary<string, int> RequestsByEndpoint { get; set; } = new();
        public Dictionary<string, int> BlockedByEndpoint { get; set; } = new();
        public Dictionary<string, int> RequestsByClient { get; set; } = new();
        public List<string> TopClients { get; set; } = new();
        public List<string> TopEndpoints { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}