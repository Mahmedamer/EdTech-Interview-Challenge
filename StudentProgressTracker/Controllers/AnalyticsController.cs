using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Services.Interfaces;
using System.Net;

namespace StudentProgressTracker.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Policy = "StudentAccess")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get class-level statistics and performance summary
        /// </summary>
        /// <param name="grade">Filter by grade (optional)</param>
        /// <param name="subject">Filter by subject name or code (optional)</param>
        /// <returns>Class summary with performance metrics</returns>
        [HttpGet("class-summary")]
        [ProducesResponseType(typeof(ApiResponseDto<AnalyticsDto.ClassSummaryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<AnalyticsDto.ClassSummaryDto>>> GetClassSummary(
            [FromQuery] int? grade = null,
            [FromQuery] string? subject = null)
        {
            try
            {
                var summary = await _analyticsService.GetClassSummaryAsync(grade, subject);

                return Ok(new ApiResponseDto<AnalyticsDto.ClassSummaryDto>
                {
                    Success = true,
                    Data = summary,
                    Message = "Class summary retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["filters"] = new { grade, subject },
                        ["generatedAt"] = DateTime.UtcNow,
                        ["dataFreshness"] = "Real-time"
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error generating class summary", 
                    new { grade, subject });
            }
        }

        /// <summary>
        /// Get historical progress data and trends
        /// </summary>
        /// <param name="startDate">Start date for trend analysis (optional, defaults to 6 months ago)</param>
        /// <param name="endDate">End date for trend analysis (optional, defaults to now)</param>
        /// <param name="period">Aggregation period: Weekly, Monthly, Quarterly (default: Monthly)</param>
        /// <returns>Progress trends over time</returns>
        [HttpGet("progress-trends")]
        [ProducesResponseType(typeof(ApiResponseDto<AnalyticsDto.ProgressTrendsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<AnalyticsDto.ProgressTrendsDto>>> GetProgressTrends(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string period = "Monthly")
        {
            try
            {
                // Validate period parameter
                var validPeriods = new[] { "Weekly", "Monthly", "Quarterly" };
                if (!validPeriods.Contains(period, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid period. Valid values are: Weekly, Monthly, Quarterly",
                        Errors = new List<string> { $"Invalid period: {period}" }
                    });
                }

                // Validate date range
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Start date cannot be later than end date",
                        Errors = new List<string> { "Invalid date range" }
                    });
                }

                var trends = await _analyticsService.GetProgressTrendsAsync(startDate, endDate, period);

                return Ok(new ApiResponseDto<AnalyticsDto.ProgressTrendsDto>
                {
                    Success = true,
                    Data = trends,
                    Message = "Progress trends retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["dateRange"] = new { startDate = startDate ?? DateTime.UtcNow.AddMonths(-6), endDate = endDate ?? DateTime.UtcNow },
                        ["period"] = period,
                        ["dataPoints"] = trends.DataPoints.Count,
                        ["generatedAt"] = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error generating progress trends", 
                    new { startDate, endDate, period });
            }
        }

        /// <summary>
        /// Get performance distribution analysis
        /// </summary>
        /// <param name="grade">Filter by grade (optional)</param>
        /// <param name="subject">Filter by subject (optional)</param>
        /// <returns>Performance distribution metrics</returns>
        [HttpGet("performance-distribution")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<object>>> GetPerformanceDistribution(
            [FromQuery] int? grade = null,
            [FromQuery] string? subject = null)
        {
            try
            {
                // Get class summary which contains the distribution data
                var summary = await _analyticsService.GetClassSummaryAsync(grade, subject);

                var distribution = new
                {
                    TotalStudents = summary.TotalStudents,
                    Distribution = new
                    {
                        Struggling = new { Count = summary.StudentsStruggling, Percentage = summary.TotalStudents > 0 ? (summary.StudentsStruggling * 100.0 / summary.TotalStudents) : 0 },
                        OnTrack = new { Count = summary.StudentsOnTrack, Percentage = summary.TotalStudents > 0 ? (summary.StudentsOnTrack * 100.0 / summary.TotalStudents) : 0 },
                        Advanced = new { Count = summary.StudentsAdvanced, Percentage = summary.TotalStudents > 0 ? (summary.StudentsAdvanced * 100.0 / summary.TotalStudents) : 0 }
                    },
                    PerformanceRanges = new[]
                    {
                        new { Range = "0-59", Label = "Needs Improvement", Color = "#ff4444" },
                        new { Range = "60-69", Label = "Below Average", Color = "#ff8800" },
                        new { Range = "70-79", Label = "Average", Color = "#ffcc00" },
                        new { Range = "80-89", Label = "Above Average", Color = "#88cc00" },
                        new { Range = "90-100", Label = "Excellent", Color = "#00cc44" }
                    }
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Data = distribution,
                    Message = "Performance distribution retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["filters"] = new { grade, subject },
                        ["generatedAt"] = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error generating performance distribution", 
                    new { grade, subject });
            }
        }

        /// <summary>
        /// Get engagement metrics and activity patterns
        /// </summary>
        /// <param name="grade">Filter by grade (optional)</param>
        /// <param name="days">Number of days to analyze (default: 30)</param>
        /// <returns>Student engagement and activity metrics</returns>
        [HttpGet("engagement-metrics")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<object>>> GetEngagementMetrics(
            [FromQuery] int? grade = null,
            [FromQuery] int days = 30)
        {
            try
            {
                if (days < 1 || days > 365)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Days parameter must be between 1 and 365",
                        Errors = new List<string> { $"Invalid days value: {days}" }
                    });
                }

                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                // Get trends for engagement analysis
                var trends = await _analyticsService.GetProgressTrendsAsync(startDate, endDate, "Weekly");
                var summary = await _analyticsService.GetClassSummaryAsync(grade);

                var engagementMetrics = new
                {
                    Period = new { StartDate = startDate, EndDate = endDate, Days = days },
                    Overview = new
                    {
                        TotalStudents = summary.TotalStudents,
                        ActiveStudents = summary.ActiveStudents,
                        EngagementRate = summary.TotalStudents > 0 ? (summary.ActiveStudents * 100.0 / summary.TotalStudents) : 0
                    },
                    ActivityTrends = trends.DataPoints.Select(dp => new
                    {
                        Date = dp.Date,
                        ActiveStudents = dp.ActiveStudents,
                        AvgPerformance = Math.Round(dp.AveragePerformance, 2),
                        AvgCompletion = Math.Round(dp.AverageCompletion, 2)
                    }).ToList(),
                    EngagementLevels = new
                    {
                        High = new { Count = summary.StudentsAdvanced, Description = "Students with 85%+ performance and 90%+ completion" },
                        Medium = new { Count = summary.StudentsOnTrack, Description = "Students with 70%+ performance and completion" },
                        Low = new { Count = summary.StudentsStruggling, Description = "Students below 70% performance or completion" }
                    }
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Data = engagementMetrics,
                    Message = "Engagement metrics retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["grade"] = grade,
                        ["period"] = $"{days} days",
                        ["generatedAt"] = DateTime.UtcNow,
                        ["dataPoints"] = trends.DataPoints.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error generating engagement metrics", 
                    new { grade, days });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Handles database-related exceptions with appropriate error responses and logging
        /// </summary>
        private ActionResult HandleDatabaseException(
            Exception ex, 
            string operationDescription, 
            object? contextData = null)
        {
            var errorId = Guid.NewGuid().ToString("N")[..8];
            var contextInfo = contextData != null ? $" Context: {System.Text.Json.JsonSerializer.Serialize(contextData)}" : "";
            
            switch (ex)
            {
                // Entity Framework specific exceptions
                case DbUpdateConcurrencyException concurrencyEx:
                    _logger.LogWarning(concurrencyEx, 
                        "[{ErrorId}] Concurrency conflict during {Operation}.{Context}", 
                        errorId, operationDescription, contextInfo);
                    
                    return Conflict(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "The record has been modified by another user. Please refresh and try again.",
                        Errors = new List<string> { "Concurrency conflict detected" },
                        Metadata = new Dictionary<string, object>
                        {
                            ["errorId"] = errorId,
                            ["errorType"] = "ConcurrencyConflict",
                            ["retryable"] = true,
                            ["timestamp"] = DateTime.UtcNow
                        }
                    });

                case DbUpdateException dbUpdateEx:
                    return HandleDbUpdateException(dbUpdateEx, operationDescription, errorId, contextInfo);

                // SQL Server specific exceptions
                case SqlException sqlEx:
                    return HandleSqlServerException(sqlEx, operationDescription, errorId, contextInfo);

                // SQLite specific exceptions
                case SqliteException sqliteEx:
                    return HandleSqliteException(sqliteEx, operationDescription, errorId, contextInfo);

                // General database connection issues
                case InvalidOperationException invalidOpEx when 
                    invalidOpEx.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                    invalidOpEx.Message.Contains("database", StringComparison.OrdinalIgnoreCase):
                    
                    _logger.LogError(invalidOpEx, 
                        "[{ErrorId}] Database connection error during {Operation}.{Context}", 
                        errorId, operationDescription, contextInfo);
                    
                    return StatusCode((int)HttpStatusCode.ServiceUnavailable, new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Database service is temporarily unavailable. Please try again later.",
                        Errors = new List<string> { "Database connection error" },
                        Metadata = new Dictionary<string, object>
                        {
                            ["errorId"] = errorId,
                            ["errorType"] = "DatabaseConnection",
                            ["retryable"] = true,
                            ["retryAfter"] = 30,
                            ["timestamp"] = DateTime.UtcNow
                        }
                    });

                // Timeout exceptions
                case TimeoutException timeoutEx:
                    _logger.LogWarning(timeoutEx, 
                        "[{ErrorId}] Operation timeout during {Operation}.{Context}", 
                        errorId, operationDescription, contextInfo);
                    
                    return StatusCode((int)HttpStatusCode.RequestTimeout, new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "The operation timed out. Please try again with a smaller request or contact support.",
                        Errors = new List<string> { "Operation timeout" },
                        Metadata = new Dictionary<string, object>
                        {
                            ["errorId"] = errorId,
                            ["errorType"] = "Timeout",
                            ["retryable"] = true,
                            ["retryAfter"] = 60,
                            ["timestamp"] = DateTime.UtcNow
                        }
                    });

                // Task cancellation (often indicates timeout or client disconnect)
                case TaskCanceledException taskCanceledEx:
                    _logger.LogWarning(taskCanceledEx, 
                        "[{ErrorId}] Task cancelled during {Operation}.{Context}", 
                        errorId, operationDescription, contextInfo);
                    
                    return StatusCode((int)HttpStatusCode.RequestTimeout, new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "The request was cancelled or timed out.",
                        Errors = new List<string> { "Request cancelled" },
                        Metadata = new Dictionary<string, object>
                        {
                            ["errorId"] = errorId,
                            ["errorType"] = "TaskCancelled",
                            ["retryable"] = true,
                            ["timestamp"] = DateTime.UtcNow
                        }
                    });

                // Generic exceptions
                default:
                    _logger.LogError(ex, 
                        "[{ErrorId}] Unexpected error during {Operation}.{Context}", 
                        errorId, operationDescription, contextInfo);
                    
                    return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "An unexpected error occurred while processing your request.",
                        Errors = new List<string> { "Internal server error" },
                        Metadata = new Dictionary<string, object>
                        {
                            ["errorId"] = errorId,
                            ["errorType"] = "UnexpectedError",
                            ["retryable"] = false,
                            ["timestamp"] = DateTime.UtcNow
                        }
                    });
            }
        }

        private ActionResult HandleSqlServerException(
            SqlException sqlEx, 
            string operationDescription, 
            string errorId, 
            string contextInfo)
        {
            var (statusCode, message, errorType, retryable) = sqlEx.Number switch
            {
                // Connection failures
                2 or 53 or 1231 or 1232 => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "Database server is currently unavailable. Please try again later.", 
                     "ConnectionFailure", true),

                // Login failures  
                18456 or 18452 => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "Database authentication failed. Please contact support.", 
                     "AuthenticationFailure", false),

                // Timeout
                -2 => 
                    (HttpStatusCode.RequestTimeout, 
                     "Database operation timed out. Please try again or contact support.", 
                     "Timeout", true),

                // Deadlock
                1205 => 
                    (HttpStatusCode.Conflict, 
                     "A database deadlock occurred. Please try your request again.", 
                     "Deadlock", true),

                // Constraint violations
                2627 or 2601 => 
                    (HttpStatusCode.Conflict, 
                     "A record with this information already exists.", 
                     "DuplicateKey", false),

                // Foreign key violations
                547 => 
                    (HttpStatusCode.BadRequest, 
                     "This operation violates data relationship constraints.", 
                     "ForeignKeyViolation", false),

                // Database full or out of space
                1105 or 9002 => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "Database storage is full. Please contact support.", 
                     "StorageFull", false),

                // Default for other SQL errors
                _ => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "A database error occurred. Please try again later.", 
                     "DatabaseError", true)
            };

            _logger.LogError(sqlEx, 
                "[{ErrorId}] SQL Server error {SqlErrorNumber} during {Operation}.{Context}", 
                errorId, sqlEx.Number, operationDescription, contextInfo);

            return StatusCode((int)statusCode, new ApiResponseDto<object>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { $"Database error {sqlEx.Number}" },
                Metadata = new Dictionary<string, object>
                {
                    ["errorId"] = errorId,
                    ["errorType"] = errorType,
                    ["sqlErrorNumber"] = sqlEx.Number,
                    ["retryable"] = retryable,
                    ["retryAfter"] = retryable ? 30 : null,
                    ["timestamp"] = DateTime.UtcNow
                }
            });
        }

        private ActionResult HandleSqliteException(
            SqliteException sqliteEx, 
            string operationDescription, 
            string errorId, 
            string contextInfo)
        {
            var (statusCode, message, errorType, retryable) = (int)sqliteEx.SqliteErrorCode switch
            {
                // Database locked (SQLITE_LOCKED = 6, SQLITE_BUSY = 5)
                5 or 6 => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "Database is temporarily locked. Please try again.", 
                     "DatabaseLocked", true),

                // Database file issues (SQLITE_CANTOPEN = 14, SQLITE_NOTADB = 26)
                14 or 26 => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "Database file cannot be accessed. Please contact support.", 
                     "FileAccess", false),

                // Constraint violations (SQLITE_CONSTRAINT = 19)
                19 => 
                    (HttpStatusCode.Conflict, 
                     "This operation violates database constraints.", 
                     "ConstraintViolation", false),

                // Disk full (SQLITE_FULL = 13)
                13 => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "Database storage is full. Please contact support.", 
                     "StorageFull", false),

                // Corruption (SQLITE_CORRUPT = 11)
                11 => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "Database corruption detected. Please contact support immediately.", 
                     "DatabaseCorruption", false),

                // Default for other SQLite errors
                _ => 
                    (HttpStatusCode.ServiceUnavailable, 
                     "A database error occurred. Please try again later.", 
                     "DatabaseError", true)
            };

            _logger.LogError(sqliteEx, 
                "[{ErrorId}] SQLite error {SqliteErrorCode} during {Operation}.{Context}", 
                errorId, sqliteEx.SqliteErrorCode, operationDescription, contextInfo);

            return StatusCode((int)statusCode, new ApiResponseDto<object>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { $"Database error {sqliteEx.SqliteErrorCode}" },
                Metadata = new Dictionary<string, object>
                {
                    ["errorId"] = errorId,
                    ["errorType"] = errorType,
                    ["sqliteErrorCode"] = sqliteEx.SqliteErrorCode.ToString(),
                    ["retryable"] = retryable,
                    ["retryAfter"] = retryable ? 30 : null,
                    ["timestamp"] = DateTime.UtcNow
                }
            });
        }

        private ActionResult HandleDbUpdateException(
            DbUpdateException dbUpdateEx, 
            string operationDescription, 
            string errorId, 
            string contextInfo)
        {
            // Check inner exception for more specific error handling
            if (dbUpdateEx.InnerException is SqlException sqlEx)
            {
                return HandleSqlServerException(sqlEx, operationDescription, errorId, contextInfo);
            }
            
            if (dbUpdateEx.InnerException is SqliteException sqliteEx)
            {
                return HandleSqliteException(sqliteEx, operationDescription, errorId, contextInfo);
            }

            _logger.LogError(dbUpdateEx, 
                "[{ErrorId}] Entity Framework update error during {Operation}.{Context}", 
                errorId, operationDescription, contextInfo);

            return StatusCode((int)HttpStatusCode.BadRequest, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to save changes to the database. Please check your data and try again.",
                Errors = new List<string> { "Database update failed" },
                Metadata = new Dictionary<string, object>
                {
                    ["errorId"] = errorId,
                    ["errorType"] = "DbUpdateError",
                    ["retryable"] = true,
                    ["timestamp"] = DateTime.UtcNow
                }
            });
        }

        #endregion
    }
}
