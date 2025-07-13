using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Services.Interfaces;
using System.Security.Claims;
using System.Net;

namespace StudentProgressTracker.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IStudentService _studentService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IStudentService studentService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>
        /// Mock authentication endpoint
        /// </summary>
        /// <param name="loginRequest">Login credentials</param>
        /// <returns>Authentication token and user profile</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthDto.LoginResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        public async Task<ActionResult<ApiResponseDto<AuthDto.LoginResponseDto>>> Login(
            [FromBody] AuthDto.LoginRequestDto loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid login data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.LoginAsync(loginRequest);
                if (result == null)
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                _logger.LogInformation("Successful login for user {Email}", loginRequest.Email);
                return Ok(new ApiResponseDto<AuthDto.LoginResponseDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error during login", 
                    new { email = loginRequest.Email });
            }
        }

        /// <summary>
        /// Get current user profile and permissions
        /// </summary>
        /// <returns>User profile information</returns>
        [HttpGet("profile")]
        [Authorize(Policy = "StudentAccess")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthDto.UserProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        public async Task<ActionResult<ApiResponseDto<AuthDto.UserProfileDto>>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid or missing user ID in token"
                    });
                }

                var profile = await _authService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "User profile not found"
                    });
                }

                return Ok(new ApiResponseDto<AuthDto.UserProfileDto>
                {
                    Success = true,
                    Data = profile,
                    Message = "Profile retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["permissions"] = GetUserPermissions(profile.Role),
                        ["features"] = GetAvailableFeatures(profile.Role)
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error retrieving user profile", 
                    new { userId = User.FindFirst("UserId")?.Value });
            }
        }

        /// <summary>
        /// Get students assigned to specific teacher
        /// </summary>
        /// <param name="id">Teacher user ID</param>
        /// <returns>List of students assigned to the teacher</returns>
        [HttpGet("users/{id}/students")]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(ApiResponseDto<List<StudentDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        public async Task<ActionResult<ApiResponseDto<List<StudentDto>>>> GetTeacherStudents(int id)
        {
            try
            {
                // Check if the current user is trying to access their own students or is an admin
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (currentUserRole != "Admin" && 
                    (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != id))
                {
                    return Forbid();
                }

                // Verify the user exists and is a teacher
                var teacherProfile = await _authService.GetUserProfileAsync(id);
                if (teacherProfile == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Teacher with ID {id} not found"
                    });
                }

                if (teacherProfile.Role != "Teacher" && teacherProfile.Role != "Admin")
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "User is not a teacher"
                    });
                }

                var students = await _studentService.GetStudentsByTeacherAsync(id);
                return Ok(new ApiResponseDto<List<StudentDto>>
                {
                    Success = true,
                    Data = students,
                    Message = "Teacher students retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["teacherId"] = id,
                        ["teacherName"] = $"{teacherProfile.FirstName} {teacherProfile.LastName}",
                        ["studentCount"] = students.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving students for teacher {id}", 
                    new { teacherId = id });
            }
        }

        /// <summary>
        /// Logout endpoint (for future token invalidation)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("logout")]
        [Authorize(Policy = "StudentAccess")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<object>>> Logout()
        {
            try
            {
                // In a real implementation, you would invalidate the token here
                // For now, we just return success
                var userIdClaim = User.FindFirst("UserId")?.Value;
                _logger.LogInformation("User {UserId} logged out", userIdClaim);

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Logout successful"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error during logout", 
                    new { userId = User.FindFirst("UserId")?.Value });
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

        private List<string> GetUserPermissions(string role)
        {
            return role switch
            {
                "Admin" => new List<string>
                {
                    "students.read", "students.create", "students.update", "students.delete",
                    "progress.read", "progress.create", "progress.update",
                    "analytics.read", "reports.export",
                    "users.read", "users.create", "users.update", "users.delete"
                },
                "Teacher" => new List<string>
                {
                    "students.read", "students.create", "students.update",
                    "progress.read", "progress.create", "progress.update",
                    "analytics.read", "reports.export"
                },
                "Student" => new List<string>
                {
                    "progress.read"
                },
                _ => new List<string>()
            };
        }

        private List<string> GetAvailableFeatures(string role)
        {
            return role switch
            {
                "Admin" => new List<string>
                {
                    "dashboard", "student-management", "progress-tracking",
                    "analytics", "reports", "user-management", "system-settings"
                },
                "Teacher" => new List<string>
                {
                    "dashboard", "student-management", "progress-tracking",
                    "analytics", "reports", "class-management"
                },
                "Student" => new List<string>
                {
                    "dashboard", "progress-view", "assignments"
                },
                _ => new List<string>()
            };
        }

        #endregion
    }
}
