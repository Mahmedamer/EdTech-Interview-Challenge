using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Services.Interfaces;
using StudentProgressTracker.Configuration;

namespace StudentProgressTracker.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class RateLimitController : ControllerBase
    {
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<RateLimitController> _logger;

        public RateLimitController(IRateLimitService rateLimitService, ILogger<RateLimitController> logger)
        {
            _rateLimitService = rateLimitService;
            _logger = logger;
        }

        /// <summary>
        /// Get rate limiting statistics and analytics
        /// </summary>
        /// <returns>Rate limiting statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponseDto<RateLimitDto.StatisticsDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<RateLimitDto.StatisticsDto>>> GetStatistics()
        {
            try
            {
                var statistics = await _rateLimitService.GetStatisticsAsync();

                var response = new RateLimitDto.StatisticsDto
                {
                    Overview = new RateLimitDto.OverviewDto
                    {
                        TotalRequests = statistics.TotalRequests,
                        BlockedRequests = statistics.BlockedRequests,
                        BlockedPercentage = Math.Round(statistics.BlockedPercentage, 2),
                        AllowedRequests = statistics.TotalRequests - statistics.BlockedRequests,
                        AllowedPercentage = Math.Round(100 - statistics.BlockedPercentage, 2)
                    },
                    Endpoints = new RateLimitDto.EndpointsDto
                    {
                        TopRequestedEndpoints = statistics.TopEndpoints,
                        RequestsByEndpoint = statistics.RequestsByEndpoint,
                        BlockedByEndpoint = statistics.BlockedByEndpoint
                    },
                    Clients = new RateLimitDto.ClientsDto
                    {
                        TopClients = statistics.TopClients,
                        RequestsByClient = statistics.RequestsByClient.Take(20).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    },
                    GeneratedAt = statistics.GeneratedAt,
                    Analytics = new RateLimitDto.AnalyticsDto
                    {
                        AverageRequestsPerClient = statistics.RequestsByClient.Count > 0 
                            ? Math.Round((double)statistics.TotalRequests / statistics.RequestsByClient.Count, 2) 
                            : 0,
                        MostActiveHour = DateTime.UtcNow.Hour, // Simplified - in real scenario, track hourly stats
                        RiskLevel = CalculateRiskLevel(statistics)
                    }
                };

                return Ok(new ApiResponseDto<RateLimitDto.StatisticsDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Rate limiting statistics retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["dataFreshness"] = "Real-time",
                        ["statisticsType"] = "Comprehensive"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rate limiting statistics");
                return StatusCode(500, new ApiResponseDto<RateLimitDto.StatisticsDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving rate limiting statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Get rate limit information for a specific client
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <returns>Client rate limit information</returns>
        [HttpGet("clients/{clientId}")]
        [ProducesResponseType(typeof(ApiResponseDto<RateLimitDto.ClientInfoDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<RateLimitDto.ClientInfoDto>>> GetClientRateLimit(string clientId)
        {
            try
            {
                var rateLimitInfo = await _rateLimitService.GetRateLimitInfoAsync(clientId);

                if (string.IsNullOrEmpty(rateLimitInfo.ClientId))
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"No rate limit data found for client {clientId}"
                    });
                }

                var response = new RateLimitDto.ClientInfoDto
                {
                    ClientId = rateLimitInfo.ClientId,
                    ClientTier = rateLimitInfo.ClientTier,
                    CurrentUsage = new RateLimitDto.CurrentUsageDto
                    {
                        RequestsInCurrentSecond = rateLimitInfo.RequestsInCurrentSecond,
                        RequestsInCurrentMinute = rateLimitInfo.RequestsInCurrentMinute,
                        RequestsInCurrentHour = rateLimitInfo.RequestsInCurrentHour,
                        RequestsInCurrentDay = rateLimitInfo.RequestsInCurrentDay,
                        ConcurrentRequests = rateLimitInfo.ConcurrentRequests
                    },
                    Violations = new RateLimitDto.ViolationsDto
                    {
                        ViolationCount = rateLimitInfo.ViolationCount,
                        PenaltyUntil = rateLimitInfo.PenaltyUntil,
                        IsUnderPenalty = rateLimitInfo.PenaltyUntil.HasValue && rateLimitInfo.PenaltyUntil > DateTime.UtcNow
                    },
                    Timing = new RateLimitDto.TimingDto
                    {
                        WindowStart = rateLimitInfo.WindowStart,
                        LastRequest = rateLimitInfo.LastRequest,
                        NextResetTime = rateLimitInfo.WindowStart.AddMinutes(1)
                    },
                    Status = DetermineClientStatus(rateLimitInfo)
                };

                return Ok(new ApiResponseDto<RateLimitDto.ClientInfoDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Client rate limit information retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rate limit info for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponseDto<RateLimitDto.ClientInfoDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving client rate limit information",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Reset rate limits for a specific client (emergency action)
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <returns>Reset confirmation</returns>
        [HttpPost("clients/{clientId}/reset")]
        [ProducesResponseType(typeof(ApiResponseDto<RateLimitDto.ClientResetDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<RateLimitDto.ClientResetDto>>> ResetClientRateLimit(string clientId)
        {
            try
            {
                // Get current info before reset for logging
                var currentInfo = await _rateLimitService.GetRateLimitInfoAsync(clientId);

                await _rateLimitService.ResetRateLimitAsync(clientId);

                _logger.LogWarning("Rate limits reset for client {ClientId} by admin {AdminUser}. " +
                                 "Previous violation count: {ViolationCount}",
                    clientId, User.Identity?.Name, currentInfo.ViolationCount);

                var response = new RateLimitDto.ClientResetDto
                {
                    ClientId = clientId,
                    ResetAt = DateTime.UtcNow,
                    ResetBy = User.Identity?.Name ?? "Unknown",
                    PreviousViolationCount = currentInfo.ViolationCount,
                    Message = $"Rate limits reset successfully for client {clientId}"
                };

                return Ok(new ApiResponseDto<RateLimitDto.ClientResetDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Rate limits reset successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["resetAt"] = DateTime.UtcNow,
                        ["resetBy"] = User.Identity?.Name ?? "Unknown",
                        ["previousViolationCount"] = currentInfo.ViolationCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting rate limits for client {ClientId}", clientId);
                return StatusCode(500, new ApiResponseDto<RateLimitDto.ClientResetDto>
                {
                    Success = false,
                    Message = "An error occurred while resetting client rate limits",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Get current rate limiting configuration
        /// </summary>
        /// <returns>Rate limiting configuration overview</returns>
        [HttpGet("configuration")]
        [ProducesResponseType(typeof(ApiResponseDto<RateLimitDto.ConfigurationDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult GetConfiguration()
        {
            try
            {
                var config = new RateLimitDto.ConfigurationDto
                {
                    Status = "Enabled", // In real scenario, get from configuration
                    DefaultLimits = new RateLimitDto.DefaultLimitsDto
                    {
                        RequestsPerSecond = 10,
                        RequestsPerMinute = 60,
                        RequestsPerHour = 3000,
                        RequestsPerDay = 50000,
                        MaxConcurrentRequests = 5
                    },
                    ClientTiers = new RateLimitDto.ClientTiersDto
                    {
                        Basic = new RateLimitDto.TierLimitsDto { RequestsPerMinute = 100, RequestsPerHour = 5000, RequestsPerDay = 100000 },
                        Standard = new RateLimitDto.TierLimitsDto { RequestsPerMinute = 300, RequestsPerHour = 15000, RequestsPerDay = 300000 },
                        Premium = new RateLimitDto.TierLimitsDto { RequestsPerMinute = 1000, RequestsPerHour = 50000, RequestsPerDay = 1000000 }
                    },
                    Features = new RateLimitDto.FeaturesDto
                    {
                        ProgressivePenalties = true,
                        ConcurrentRequestLimiting = true,
                        IpWhitelisting = true,
                        ClientTierSupport = true,
                        DetailedStatistics = true
                    },
                    Storage = new RateLimitDto.StorageDto
                    {
                        Type = "InMemory",
                        DataRetention = "1 day"
                    }
                };

                return Ok(new ApiResponseDto<RateLimitDto.ConfigurationDto>
                {
                    Success = true,
                    Data = config,
                    Message = "Rate limiting configuration retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rate limiting configuration");
                return StatusCode(500, new ApiResponseDto<RateLimitDto.ConfigurationDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving configuration",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Simulate rate limiting scenarios for testing
        /// </summary>
        /// <param name="scenario">Simulation scenario</param>
        /// <returns>Simulation results</returns>
        [HttpPost("simulate")]
        [ProducesResponseType(typeof(ApiResponseDto<RateLimitDto.SimulationResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<RateLimitDto.SimulationResultDto>>> SimulateScenario([FromBody] RateLimitDto.SimulationRequestDto scenario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid simulation request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var results = await RunSimulation(scenario);

                return Ok(new ApiResponseDto<RateLimitDto.SimulationResultDto>
                {
                    Success = true,
                    Data = results,
                    Message = "Rate limiting simulation completed successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["simulationTime"] = DateTime.UtcNow,
                        ["scenario"] = scenario.ScenarioName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running rate limiting simulation");
                return StatusCode(500, new ApiResponseDto<RateLimitDto.SimulationResultDto>
                {
                    Success = false,
                    Message = "An error occurred while running simulation",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #region Private Methods

        private string CalculateRiskLevel(RateLimitStatistics statistics)
        {
            if (statistics.TotalRequests == 0) return "Low";
            
            var blockedPercentage = statistics.BlockedPercentage;
            
            return blockedPercentage switch
            {
                >= 20 => "Critical",
                >= 10 => "High",
                >= 5 => "Medium",
                _ => "Low"
            };
        }

        private string DetermineClientStatus(RateLimitInfo rateLimitInfo)
        {
            if (rateLimitInfo.PenaltyUntil.HasValue && rateLimitInfo.PenaltyUntil > DateTime.UtcNow)
            {
                return "Under Penalty";
            }

            if (rateLimitInfo.ViolationCount > 0)
            {
                return "Previous Violations";
            }

            if (rateLimitInfo.RequestsInCurrentMinute > 50) // Arbitrary threshold
            {
                return "High Activity";
            }

            return "Normal";
        }

        private async Task<RateLimitDto.SimulationResultDto> RunSimulation(RateLimitDto.SimulationRequestDto scenario)
        {
            var results = new RateLimitDto.SimulationResultDto
            {
                ScenarioName = scenario.ScenarioName,
                Description = scenario.Description,
                Parameters = scenario,
                Results = scenario.ScenarioName.ToLower() switch
                {
                    "burst_test" => await SimulateBurstTest(scenario),
                    "sustained_load" => await SimulateSustainedLoad(scenario),
                    "multiple_clients" => await SimulateMultipleClients(scenario),
                    _ => new { message = "Unknown scenario type" }
                },
                Recommendations = GenerateRecommendations(scenario)
            };

            return results;
        }

        private async Task<RateLimitDto.BurstTestResultDto> SimulateBurstTest(RateLimitDto.SimulationRequestDto scenario)
        {
            var allowedRequests = 0;
            var blockedRequests = 0;
            var testClientId = $"simulation_burst_{Guid.NewGuid():N}";

            // Simulate rapid requests
            for (int i = 0; i < scenario.RequestCount; i++)
            {
                var result = await _rateLimitService.CheckRateLimitAsync(testClientId, "/api/test", "127.0.0.1");
                
                if (result.IsAllowed)
                {
                    allowedRequests++;
                    await _rateLimitService.RecordRequestAsync(testClientId, "/api/test", "127.0.0.1");
                }
                else
                {
                    blockedRequests++;
                }

                // Small delay to simulate realistic timing
                await Task.Delay(scenario.DelayBetweenRequestsMs);
            }

            return new RateLimitDto.BurstTestResultDto
            {
                TotalRequests = scenario.RequestCount,
                AllowedRequests = allowedRequests,
                BlockedRequests = blockedRequests,
                SuccessRate = Math.Round((double)allowedRequests / scenario.RequestCount * 100, 2),
                Description = $"Burst test with {scenario.RequestCount} requests sent rapidly"
            };
        }

        private async Task<RateLimitDto.SustainedLoadResultDto> SimulateSustainedLoad(RateLimitDto.SimulationRequestDto scenario)
        {
            var results = new List<RateLimitDto.MinuteResultDto>();
            var testClientId = $"simulation_sustained_{Guid.NewGuid():N}";

            // Simulate requests over time
            var startTime = DateTime.UtcNow;
            var allowedCount = 0;
            var blockedCount = 0;

            for (int minute = 0; minute < scenario.DurationMinutes; minute++)
            {
                var minuteAllowed = 0;
                var minuteBlocked = 0;

                for (int i = 0; i < scenario.RequestsPerMinute; i++)
                {
                    var result = await _rateLimitService.CheckRateLimitAsync(testClientId, "/api/test", "127.0.0.1");
                    
                    if (result.IsAllowed)
                    {
                        minuteAllowed++;
                        allowedCount++;
                        await _rateLimitService.RecordRequestAsync(testClientId, "/api/test", "127.0.0.1");
                    }
                    else
                    {
                        minuteBlocked++;
                        blockedCount++;
                    }

                    await Task.Delay(60000 / scenario.RequestsPerMinute); // Distribute requests evenly across minute
                }

                results.Add(new RateLimitDto.MinuteResultDto
                {
                    Minute = minute + 1,
                    Allowed = minuteAllowed,
                    Blocked = minuteBlocked,
                    SuccessRate = Math.Round((double)minuteAllowed / (minuteAllowed + minuteBlocked) * 100, 2)
                });
            }

            return new RateLimitDto.SustainedLoadResultDto
            {
                Duration = $"{scenario.DurationMinutes} minutes",
                TotalRequests = allowedCount + blockedCount,
                TotalAllowed = allowedCount,
                TotalBlocked = blockedCount,
                OverallSuccessRate = Math.Round((double)allowedCount / (allowedCount + blockedCount) * 100, 2),
                MinuteByMinuteResults = results
            };
        }

        private async Task<RateLimitDto.MultipleClientsResultDto> SimulateMultipleClients(RateLimitDto.SimulationRequestDto scenario)
        {
            var clientResults = new List<RateLimitDto.ClientResultDto>();
            var tasks = new List<Task>();

            for (int clientIndex = 0; clientIndex < scenario.ClientCount; clientIndex++)
            {
                var clientId = $"simulation_client_{clientIndex}_{Guid.NewGuid():N}";
                
                tasks.Add(Task.Run(async () =>
                {
                    var allowedRequests = 0;
                    var blockedRequests = 0;

                    for (int i = 0; i < scenario.RequestsPerClient; i++)
                    {
                        var result = await _rateLimitService.CheckRateLimitAsync(clientId, "/api/test", $"127.0.0.{clientIndex + 1}");
                        
                        if (result.IsAllowed)
                        {
                            allowedRequests++;
                            await _rateLimitService.RecordRequestAsync(clientId, "/api/test", $"127.0.0.{clientIndex + 1}");
                        }
                        else
                        {
                            blockedRequests++;
                        }

                        await Task.Delay(scenario.DelayBetweenRequestsMs);
                    }

                    lock (clientResults)
                    {
                        clientResults.Add(new RateLimitDto.ClientResultDto
                        {
                            ClientId = clientId,
                            AllowedRequests = allowedRequests,
                            BlockedRequests = blockedRequests,
                            SuccessRate = Math.Round((double)allowedRequests / (allowedRequests + blockedRequests) * 100, 2)
                        });
                    }
                }));
            }

            await Task.WhenAll(tasks);

            var totalAllowed = clientResults.Sum(r => r.AllowedRequests);
            var totalBlocked = clientResults.Sum(r => r.BlockedRequests);

            return new RateLimitDto.MultipleClientsResultDto
            {
                ClientCount = scenario.ClientCount,
                RequestsPerClient = scenario.RequestsPerClient,
                TotalRequests = totalAllowed + totalBlocked,
                TotalAllowed = totalAllowed,
                TotalBlocked = totalBlocked,
                OverallSuccessRate = Math.Round((double)totalAllowed / (totalAllowed + totalBlocked) * 100, 2),
                ClientResults = clientResults.Take(10).ToList() // Limit displayed results
            };
        }

        private List<string> GenerateRecommendations(RateLimitDto.SimulationRequestDto scenario)
        {
            var recommendations = new List<string>();

            switch (scenario.ScenarioName.ToLower())
            {
                case "burst_test":
                    recommendations.Add("Consider implementing token bucket algorithm for better burst handling");
                    recommendations.Add("Monitor client applications for retry logic implementation");
                    break;
                    
                case "sustained_load":
                    recommendations.Add("Ensure rate limits align with expected sustained usage patterns");
                    recommendations.Add("Consider implementing graduated rate limits based on client tier");
                    break;
                    
                case "multiple_clients":
                    recommendations.Add("Monitor for fair distribution of rate limits across clients");
                    recommendations.Add("Consider per-IP rate limiting in addition to per-client limits");
                    break;
            }

            recommendations.Add("Implement monitoring and alerting for rate limit violations");
            recommendations.Add("Provide clear rate limit documentation for API consumers");

            return recommendations;
        }

        #endregion
    }
}