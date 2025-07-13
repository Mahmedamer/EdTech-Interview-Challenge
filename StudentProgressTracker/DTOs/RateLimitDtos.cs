using System.ComponentModel.DataAnnotations;

namespace StudentProgressTracker.DTOs
{
    /// <summary>
    /// Rate limiting related DTOs
    /// </summary>
    public class RateLimitDto
    {
        /// <summary>
        /// Rate limiting statistics overview
        /// </summary>
        public class StatisticsDto
        {
            public OverviewDto Overview { get; set; } = new();
            public EndpointsDto Endpoints { get; set; } = new();
            public ClientsDto Clients { get; set; } = new();
            public DateTime GeneratedAt { get; set; }
            public AnalyticsDto Analytics { get; set; } = new();
        }

        /// <summary>
        /// Overview statistics
        /// </summary>
        public class OverviewDto
        {
            [Range(0, int.MaxValue)]
            public int TotalRequests { get; set; }
            
            [Range(0, int.MaxValue)]
            public int BlockedRequests { get; set; }
            
            [Range(0, 100)]
            public double BlockedPercentage { get; set; }
            
            [Range(0, int.MaxValue)]
            public int AllowedRequests { get; set; }
            
            [Range(0, 100)]
            public double AllowedPercentage { get; set; }
        }

        /// <summary>
        /// Endpoint-specific statistics
        /// </summary>
        public class EndpointsDto
        {
            public List<string> TopRequestedEndpoints { get; set; } = new();
            public Dictionary<string, int> RequestsByEndpoint { get; set; } = new();
            public Dictionary<string, int> BlockedByEndpoint { get; set; } = new();
        }

        /// <summary>
        /// Client-specific statistics
        /// </summary>
        public class ClientsDto
        {
            public List<string> TopClients { get; set; } = new();
            public Dictionary<string, int> RequestsByClient { get; set; } = new();
        }

        /// <summary>
        /// Analytics data for rate limiting
        /// </summary>
        public class AnalyticsDto
        {
            [Range(0, double.MaxValue)]
            public double AverageRequestsPerClient { get; set; }
            
            [Range(0, 23)]
            public int MostActiveHour { get; set; }
            
            [Required]
            [StringLength(20)]
            public string RiskLevel { get; set; } = string.Empty;
        }

        /// <summary>
        /// Client rate limit information
        /// </summary>
        public class ClientInfoDto
        {
            [Required]
            [StringLength(100, MinimumLength = 1)]
            public string ClientId { get; set; } = string.Empty;
            
            [Required]
            [StringLength(20)]
            public string ClientTier { get; set; } = string.Empty;
            
            public CurrentUsageDto CurrentUsage { get; set; } = new();
            public ViolationsDto Violations { get; set; } = new();
            public TimingDto Timing { get; set; } = new();
            
            [Required]
            [StringLength(50)]
            public string Status { get; set; } = string.Empty;
        }

        /// <summary>
        /// Current usage information for a client
        /// </summary>
        public class CurrentUsageDto
        {
            [Range(0, int.MaxValue)]
            public int RequestsInCurrentSecond { get; set; }
            
            [Range(0, int.MaxValue)]
            public int RequestsInCurrentMinute { get; set; }
            
            [Range(0, int.MaxValue)]
            public int RequestsInCurrentHour { get; set; }
            
            [Range(0, int.MaxValue)]
            public int RequestsInCurrentDay { get; set; }
            
            [Range(0, int.MaxValue)]
            public int ConcurrentRequests { get; set; }
        }

        /// <summary>
        /// Violation information for a client
        /// </summary>
        public class ViolationsDto
        {
            [Range(0, int.MaxValue)]
            public int ViolationCount { get; set; }
            
            public DateTime? PenaltyUntil { get; set; }
            public bool IsUnderPenalty { get; set; }
        }

        /// <summary>
        /// Timing information for rate limit windows
        /// </summary>
        public class TimingDto
        {
            public DateTime WindowStart { get; set; }
            public DateTime LastRequest { get; set; }
            public DateTime NextResetTime { get; set; }
        }

        /// <summary>
        /// Rate limiting configuration information
        /// </summary>
        public class ConfigurationDto
        {
            [Required]
            [StringLength(20)]
            public string Status { get; set; } = string.Empty;
            
            public DefaultLimitsDto DefaultLimits { get; set; } = new();
            public ClientTiersDto ClientTiers { get; set; } = new();
            public FeaturesDto Features { get; set; } = new();
            public StorageDto Storage { get; set; } = new();
        }

        /// <summary>
        /// Default rate limits configuration
        /// </summary>
        public class DefaultLimitsDto
        {
            [Range(1, 1000)]
            public int RequestsPerSecond { get; set; }
            
            [Range(1, 10000)]
            public int RequestsPerMinute { get; set; }
            
            [Range(1, 1000000)]
            public int RequestsPerHour { get; set; }
            
            [Range(1, 50000000)]
            public int RequestsPerDay { get; set; }
            
            [Range(1, 1000)]
            public int MaxConcurrentRequests { get; set; }
        }

        /// <summary>
        /// Client tiers configuration
        /// </summary>
        public class ClientTiersDto
        {
            public TierLimitsDto Basic { get; set; } = new();
            public TierLimitsDto Standard { get; set; } = new();
            public TierLimitsDto Premium { get; set; } = new();
        }

        /// <summary>
        /// Tier-specific limits
        /// </summary>
        public class TierLimitsDto
        {
            [Range(1, 10000)]
            public int RequestsPerMinute { get; set; }
            
            [Range(1, 1000000)]
            public int RequestsPerHour { get; set; }
            
            [Range(1, 50000000)]
            public int RequestsPerDay { get; set; }
        }

        /// <summary>
        /// Rate limiting features configuration
        /// </summary>
        public class FeaturesDto
        {
            public bool ProgressivePenalties { get; set; }
            public bool ConcurrentRequestLimiting { get; set; }
            public bool IpWhitelisting { get; set; }
            public bool ClientTierSupport { get; set; }
            public bool DetailedStatistics { get; set; }
        }

        /// <summary>
        /// Storage configuration information
        /// </summary>
        public class StorageDto
        {
            [Required]
            [StringLength(50)]
            public string Type { get; set; } = string.Empty;
            
            [Required]
            [StringLength(100)]
            public string DataRetention { get; set; } = string.Empty;
        }

        /// <summary>
        /// Client reset confirmation
        /// </summary>
        public class ClientResetDto
        {
            [Required]
            [StringLength(100, MinimumLength = 1)]
            public string ClientId { get; set; } = string.Empty;
            
            public DateTime ResetAt { get; set; }
            
            [Required]
            [StringLength(100)]
            public string ResetBy { get; set; } = string.Empty;
            
            [Range(0, int.MaxValue)]
            public int PreviousViolationCount { get; set; }
            
            [Required]
            [StringLength(200)]
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// Simulation request parameters
        /// </summary>
        public class SimulationRequestDto
        {
            [Required]
            [StringLength(50, MinimumLength = 1)]
            public string ScenarioName { get; set; } = "burst_test";
            
            [StringLength(500)]
            public string Description { get; set; } = string.Empty;
            
            [Range(1, 10000)]
            public int RequestCount { get; set; } = 100;
            
            [Range(1, 10000)]
            public int DelayBetweenRequestsMs { get; set; } = 10;
            
            [Range(1, 60)]
            public int DurationMinutes { get; set; } = 5;
            
            [Range(1, 1000)]
            public int RequestsPerMinute { get; set; } = 60;
            
            [Range(1, 100)]
            public int ClientCount { get; set; } = 10;
            
            [Range(1, 1000)]
            public int RequestsPerClient { get; set; } = 50;
        }

        /// <summary>
        /// Simulation results
        /// </summary>
        public class SimulationResultDto
        {
            [Required]
            [StringLength(50)]
            public string ScenarioName { get; set; } = string.Empty;
            
            [StringLength(500)]
            public string Description { get; set; } = string.Empty;
            
            public SimulationRequestDto Parameters { get; set; } = new();
            public object Results { get; set; } = new();
            public List<string> Recommendations { get; set; } = new();
        }

        /// <summary>
        /// Burst test simulation results
        /// </summary>
        public class BurstTestResultDto
        {
            [Range(0, int.MaxValue)]
            public int TotalRequests { get; set; }
            
            [Range(0, int.MaxValue)]
            public int AllowedRequests { get; set; }
            
            [Range(0, int.MaxValue)]
            public int BlockedRequests { get; set; }
            
            [Range(0, 100)]
            public double SuccessRate { get; set; }
            
            [StringLength(200)]
            public string Description { get; set; } = string.Empty;
        }

        /// <summary>
        /// Sustained load simulation results
        /// </summary>
        public class SustainedLoadResultDto
        {
            [Required]
            [StringLength(50)]
            public string Duration { get; set; } = string.Empty;
            
            [Range(0, int.MaxValue)]
            public int TotalRequests { get; set; }
            
            [Range(0, int.MaxValue)]
            public int TotalAllowed { get; set; }
            
            [Range(0, int.MaxValue)]
            public int TotalBlocked { get; set; }
            
            [Range(0, 100)]
            public double OverallSuccessRate { get; set; }
            
            public List<MinuteResultDto> MinuteByMinuteResults { get; set; } = new();
        }

        /// <summary>
        /// Per-minute results for sustained load testing
        /// </summary>
        public class MinuteResultDto
        {
            [Range(1, int.MaxValue)]
            public int Minute { get; set; }
            
            [Range(0, int.MaxValue)]
            public int Allowed { get; set; }
            
            [Range(0, int.MaxValue)]
            public int Blocked { get; set; }
            
            [Range(0, 100)]
            public double SuccessRate { get; set; }
        }

        /// <summary>
        /// Multiple clients simulation results
        /// </summary>
        public class MultipleClientsResultDto
        {
            [Range(1, int.MaxValue)]
            public int ClientCount { get; set; }
            
            [Range(1, int.MaxValue)]
            public int RequestsPerClient { get; set; }
            
            [Range(0, int.MaxValue)]
            public int TotalRequests { get; set; }
            
            [Range(0, int.MaxValue)]
            public int TotalAllowed { get; set; }
            
            [Range(0, int.MaxValue)]
            public int TotalBlocked { get; set; }
            
            [Range(0, 100)]
            public double OverallSuccessRate { get; set; }
            
            public List<ClientResultDto> ClientResults { get; set; } = new();
        }

        /// <summary>
        /// Individual client results for multiple clients testing
        /// </summary>
        public class ClientResultDto
        {
            [Required]
            [StringLength(100)]
            public string ClientId { get; set; } = string.Empty;
            
            [Range(0, int.MaxValue)]
            public int AllowedRequests { get; set; }
            
            [Range(0, int.MaxValue)]
            public int BlockedRequests { get; set; }
            
            [Range(0, 100)]
            public double SuccessRate { get; set; }
        }
    }
}