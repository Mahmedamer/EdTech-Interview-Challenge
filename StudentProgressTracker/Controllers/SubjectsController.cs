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
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly ILogger<SubjectsController> _logger;

        public SubjectsController(ISubjectService subjectService, ILogger<SubjectsController> logger)
        {
            _subjectService = subjectService;
            _logger = logger;
        }

        /// <summary>
        /// Get all subjects with filtering, pagination, and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="grade">Filter by grade</param>
        /// <param name="searchTerm">Search in name, code, or description</param>
        /// <param name="sortBy">Sort by: name, code, grade, createdate, students</param>
        /// <param name="ascending">Sort direction (default: true)</param>
        /// <param name="isActive">Filter by active status</param>
        /// <returns>Paginated list of subjects</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResponseDto<SubjectDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<SubjectDto>>>> GetSubjects(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? grade = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var result = await _subjectService.GetSubjectsAsync(
                    page, pageSize, grade, searchTerm, sortBy, ascending, isActive);

                return Ok(new ApiResponseDto<PaginatedResponseDto<SubjectDto>>
                {
                    Success = true,
                    Data = result,
                    Message = $"Retrieved {result.Data.Count} subjects (page {page} of {result.TotalPages})",
                    Metadata = new Dictionary<string, object>
                    {
                        ["pagination"] = new 
                        { 
                            result.Page, 
                            result.PageSize, 
                            result.TotalCount, 
                            result.TotalPages,
                            result.HasNextPage,
                            result.HasPreviousPage
                        },
                        ["filters"] = new { grade, searchTerm, sortBy, ascending, isActive }
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error retrieving subjects", 
                    new { page, pageSize, grade, searchTerm, sortBy, ascending, isActive });
            }
        }

        /// <summary>
        /// Get subjects by grade
        /// </summary>
        /// <param name="grade">Grade number (1-12)</param>
        /// <returns>List of subjects for the specified grade</returns>
        [HttpGet("grade/{grade}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<SubjectDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<List<SubjectDto>>>> GetSubjectsByGrade(int grade)
        {
            try
            {
                if (grade < 1 || grade > 12)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Grade must be between 1 and 12",
                        Errors = new List<string> { $"Invalid grade: {grade}" }
                    });
                }

                var subjects = await _subjectService.GetSubjectsByGradeAsync(grade);

                return Ok(new ApiResponseDto<List<SubjectDto>>
                {
                    Success = true,
                    Data = subjects,
                    Message = $"Retrieved {subjects.Count} subjects for grade {grade}",
                    Metadata = new Dictionary<string, object>
                    {
                        ["grade"] = grade,
                        ["totalSubjects"] = subjects.Count,
                        ["activeSubjects"] = subjects.Count(s => s.IsActive)
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving subjects for grade {grade}", 
                    new { grade });
            }
        }

        /// <summary>
        /// Get a specific subject by ID
        /// </summary>
        /// <param name="id">Subject ID</param>
        /// <returns>Subject details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<SubjectDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<SubjectDto>>> GetSubject(int id)
        {
            try
            {
                var subject = await _subjectService.GetSubjectByIdAsync(id);

                if (subject == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Subject with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<SubjectDto>
                {
                    Success = true,
                    Data = subject,
                    Message = "Subject retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving subject {id}", 
                    new { subjectId = id });
            }
        }

        /// <summary>
        /// Get detailed subject information including assignments and progress
        /// </summary>
        /// <param name="id">Subject ID</param>
        /// <returns>Detailed subject information</returns>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponseDto<SubjectDetailsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<SubjectDetailsDto>>> GetSubjectDetails(int id)
        {
            try
            {
                var subjectDetails = await _subjectService.GetSubjectDetailsAsync(id);

                if (subjectDetails == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Subject with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<SubjectDetailsDto>
                {
                    Success = true,
                    Data = subjectDetails,
                    Message = "Subject details retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["assignmentCount"] = subjectDetails.Assignments.Count,
                        ["recentProgressCount"] = subjectDetails.RecentProgress.Count,
                        ["statisticsIncluded"] = true
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving subject details for {id}", 
                    new { subjectId = id });
            }
        }

        /// <summary>
        /// Get subject statistics
        /// </summary>
        /// <param name="id">Subject ID</param>
        /// <returns>Subject statistics and performance metrics</returns>
        [HttpGet("{id}/statistics")]
        [ProducesResponseType(typeof(ApiResponseDto<SubjectStatisticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<SubjectStatisticsDto>>> GetSubjectStatistics(int id)
        {
            try
            {
                var statistics = await _subjectService.GetSubjectStatisticsAsync(id);

                return Ok(new ApiResponseDto<SubjectStatisticsDto>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Subject statistics retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["generatedAt"] = DateTime.UtcNow,
                        ["subjectId"] = id
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving statistics for subject {id}", 
                    new { subjectId = id });
            }
        }

        /// <summary>
        /// Create a new subject
        /// </summary>
        /// <param name="createDto">Subject creation data</param>
        /// <returns>Created subject</returns>
        [HttpPost]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(ApiResponseDto<SubjectDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<SubjectDto>>> CreateSubject([FromBody] SubjectCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid subject data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var subject = await _subjectService.CreateSubjectAsync(createDto);

                return CreatedAtAction(
                    nameof(GetSubject),
                    new { id = subject.Id },
                    new ApiResponseDto<SubjectDto>
                    {
                        Success = true,
                        Data = subject,
                        Message = $"Subject '{subject.Name}' created successfully"
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error creating subject", 
                    new { subjectData = createDto });
            }
        }

        /// <summary>
        /// Update an existing subject
        /// </summary>
        /// <param name="id">Subject ID</param>
        /// <param name="updateDto">Subject update data</param>
        /// <returns>Updated subject</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(ApiResponseDto<SubjectDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<SubjectDto>>> UpdateSubject(int id, [FromBody] SubjectUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid subject data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var subject = await _subjectService.UpdateSubjectAsync(id, updateDto);

                if (subject == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Subject with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<SubjectDto>
                {
                    Success = true,
                    Data = subject,
                    Message = $"Subject '{subject.Name}' updated successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error updating subject {id}", 
                    new { subjectId = id, updateData = updateDto });
            }
        }

        /// <summary>
        /// Delete a subject (soft delete if has associated data)
        /// </summary>
        /// <param name="id">Subject ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteSubject(int id)
        {
            try
            {
                var result = await _subjectService.DeleteSubjectAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Subject with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Subject deleted successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["deletedId"] = id,
                        ["deletedAt"] = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error deleting subject {id}", 
                    new { subjectId = id });
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