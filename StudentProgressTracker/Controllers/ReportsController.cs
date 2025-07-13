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
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Export student data in various formats (CSV, Excel, PDF)
        /// </summary>
        /// <param name="format">Export format: csv, excel, pdf (default: csv)</param>
        /// <param name="grade">Filter by grade (optional)</param>
        /// <param name="subject">Filter by subject (optional)</param>
        /// <param name="includeProgress">Include progress data (default: true)</param>
        /// <param name="includeAssignments">Include assignment data (default: false)</param>
        /// <returns>File download with student data</returns>
        [HttpGet("export/students")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ExportStudentData(
            [FromQuery] string format = "csv",
            [FromQuery] int? grade = null,
            [FromQuery] string? subject = null,
            [FromQuery] bool includeProgress = true,
            [FromQuery] bool includeAssignments = false)
        {
            try
            {
                // Validate format
                var validFormats = new[] { "csv", "excel", "pdf" };
                if (!validFormats.Contains(format.ToLower()))
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid format. Valid formats are: csv, excel, pdf",
                        Errors = new List<string> { $"Invalid format: {format}" }
                    });
                }

                var exportOptions = new ReportDto.ExportOptionsDto
                {
                    Format = format.ToLower(),
                    Grade = grade,
                    Subject = subject,
                    IncludeProgress = includeProgress,
                    IncludeAssignments = includeAssignments
                };

                var exportResult = await _reportService.ExportStudentDataAsync(exportOptions);

                var contentType = format.ToLower() switch
                {
                    "csv" => "text/csv",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };

                var fileExtension = format.ToLower() switch
                {
                    "csv" => "csv",
                    "excel" => "xlsx",
                    "pdf" => "pdf",
                    _ => "bin"
                };

                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"student-data-export_{timestamp}.{fileExtension}";

                _logger.LogInformation("Student data exported: {FileName}, Format: {Format}, Records: {RecordCount}", 
                    fileName, format, exportResult.RecordCount);

                return File(exportResult.Data, contentType, fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error exporting student data", 
                    new { format, grade, subject, includeProgress, includeAssignments });
            }
        }

        /// <summary>
        /// Generate a comprehensive progress report for a specific student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="format">Report format: pdf, excel (default: pdf)</param>
        /// <param name="includeHistory">Include historical progress data (default: true)</param>
        /// <param name="includeRecommendations">Include recommendations (default: true)</param>
        /// <returns>Student progress report file</returns>
        [HttpGet("student/{studentId}/progress-report")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GenerateStudentProgressReport(
            int studentId,
            [FromQuery] string format = "pdf",
            [FromQuery] bool includeHistory = true,
            [FromQuery] bool includeRecommendations = true)
        {
            try
            {
                // Validate format
                var validFormats = new[] { "pdf", "excel" };
                if (!validFormats.Contains(format.ToLower()))
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid format. Valid formats are: pdf, excel",
                        Errors = new List<string> { $"Invalid format: {format}" }
                    });
                }

                var reportOptions = new ReportDto.StudentReportOptionsDto
                {
                    StudentId = studentId,
                    Format = format.ToLower(),
                    IncludeHistory = includeHistory,
                    IncludeRecommendations = includeRecommendations
                };

                var reportResult = await _reportService.GenerateStudentProgressReportAsync(reportOptions);
                
                if (reportResult == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Student not found",
                        Errors = new List<string> { $"No student found with ID: {studentId}" }
                    });
                }

                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream"
                };

                var fileExtension = format.ToLower() switch
                {
                    "pdf" => "pdf",
                    "excel" => "xlsx",
                    _ => "bin"
                };

                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"progress-report_student-{studentId}_{timestamp}.{fileExtension}";

                _logger.LogInformation("Student progress report generated: {FileName}, Student: {StudentId}, Format: {Format}", 
                    fileName, studentId, format);

                return File(reportResult.Data, contentType, fileName);
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error generating progress report for student {studentId}", 
                    new { studentId, format, includeHistory, includeRecommendations });
            }
        }

        /// <summary>
        /// Generate a class summary report for teachers
        /// </summary>
        /// <param name="format">Report format: pdf, excel (default: pdf)</param>
        /// <param name="grade">Filter by grade (optional)</param>
        /// <param name="subject">Filter by subject (optional)</param>
        /// <param name="includeAnalytics">Include analytics and charts (default: true)</param>
        /// <returns>Class summary report file</returns>
        [HttpGet("class-summary")]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GenerateClassSummaryReport(
            [FromQuery] string format = "pdf",
            [FromQuery] int? grade = null,
            [FromQuery] string? subject = null,
            [FromQuery] bool includeAnalytics = true)
        {
            try
            {
                // Validate format
                var validFormats = new[] { "pdf", "excel" };
                if (!validFormats.Contains(format.ToLower()))
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid format. Valid formats are: pdf, excel",
                        Errors = new List<string> { $"Invalid format: {format}" }
                    });
                }

                var reportOptions = new ReportDto.ClassReportOptionsDto
                {
                    Format = format.ToLower(),
                    Grade = grade,
                    Subject = subject,
                    IncludeAnalytics = includeAnalytics
                };

                var reportResult = await _reportService.GenerateClassSummaryReportAsync(reportOptions);

                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream"
                };

                var fileExtension = format.ToLower() switch
                {
                    "pdf" => "pdf",
                    "excel" => "xlsx",
                    _ => "bin"
                };

                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"class-summary_{timestamp}.{fileExtension}";

                _logger.LogInformation("Class summary report generated: {FileName}, Grade: {Grade}, Subject: {Subject}, Format: {Format}", 
                    fileName, grade, subject, format);

                return File(reportResult.Data, contentType, fileName);
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error generating class summary report", 
                    new { format, grade, subject, includeAnalytics });
            }
        }

        /// <summary>
        /// Get available report templates and formats
        /// </summary>
        /// <returns>List of available report types and formats</returns>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public IActionResult GetReportTemplates()
        {
            try
            {
                var templates = new
                {
                    StudentReports = new[]
                    {
                        new
                        {
                            Name = "Progress Report",
                            Description = "Comprehensive individual student progress analysis",
                            Endpoint = "/api/reports/student/{studentId}/progress-report",
                            Formats = new[] { "pdf", "excel" },
                            Options = new[] { "includeHistory", "includeRecommendations" }
                        }
                    },
                    ClassReports = new[]
                    {
                        new
                        {
                            Name = "Class Summary",
                            Description = "Overview of class performance and analytics",
                            Endpoint = "/api/reports/class-summary",
                            Formats = new[] { "pdf", "excel" },
                            Options = new[] { "includeAnalytics" },
                            RequiredRoles = new[] { "Admin", "Teacher" }
                        }
                    },
                    DataExports = new[]
                    {
                        new
                        {
                            Name = "Student Data Export",
                            Description = "Export student data in various formats",
                            Endpoint = "/api/reports/export/students",
                            Formats = new[] { "csv", "excel", "pdf" },
                            Options = new[] { "includeProgress", "includeAssignments" },
                            Filters = new[] { "grade", "subject" }
                        }
                    },
                    SupportedFormats = new
                    {
                        CSV = new { Extension = "csv", ContentType = "text/csv", Description = "Comma-separated values" },
                        Excel = new { Extension = "xlsx", ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Description = "Microsoft Excel format" },
                        PDF = new { Extension = "pdf", ContentType = "application/pdf", Description = "Portable Document Format" }
                    }
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Data = templates,
                    Message = "Report templates retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error retrieving report templates");
            }
        }

        /// <summary>
        /// Get report generation status and history
        /// </summary>
        /// <param name="days">Number of days to look back (default: 7)</param>
        /// <returns>Report generation history and statistics</returns>
        [HttpGet("history")]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult GetReportHistory([FromQuery] int days = 7)
        {
            try
            {
                // This would typically come from a database log table
                // For now, return a mock response structure
                var history = new
                {
                    Period = new { Days = days, StartDate = DateTime.UtcNow.AddDays(-days), EndDate = DateTime.UtcNow },
                    Statistics = new
                    {
                        TotalReports = 0, // Would be calculated from database
                        ReportsByType = new
                        {
                            StudentReports = 0,
                            ClassReports = 0,
                            DataExports = 0
                        },
                        ReportsByFormat = new
                        {
                            PDF = 0,
                            Excel = 0,
                            CSV = 0
                        }
                    },
                    RecentReports = new object[0], // Would be populated from database
                    Message = "Report history feature will be implemented with database logging"
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Data = history,
                    Message = "Report history retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["note"] = "This is a placeholder implementation. Full history tracking requires database schema updates."
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error retrieving report history", 
                    new { days });
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
