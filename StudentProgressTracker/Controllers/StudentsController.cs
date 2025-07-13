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
    [Authorize(Policy = "StudentAccess")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all students with filtering, pagination, and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="grade">Filter by grade</param>
        /// <param name="subject">Filter by subject name or code</param>
        /// <param name="startDate">Filter by progress start date</param>
        /// <param name="endDate">Filter by progress end date</param>
        /// <param name="searchTerm">Search in name or email</param>
        /// <param name="sortBy">Sort by: name, progress, lastactivity</param>
        /// <param name="ascending">Sort direction (default: true)</param>
        /// <returns>Paginated list of students</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResponseDto<StudentDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<StudentDto>>>> GetStudents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? grade = null,
            [FromQuery] string? subject = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var result = await _studentService.GetStudentsAsync(
                    page, pageSize, grade, subject, startDate, endDate, searchTerm, sortBy, ascending);

                return Ok(new ApiResponseDto<PaginatedResponseDto<StudentDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Students retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error retrieving students", 
                    new { page, pageSize, grade, subject, searchTerm });
            }
        }

        /// <summary>
        /// Get a student by ID
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <returns>Student details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<StudentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<StudentDto>>> GetStudent(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Student with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<StudentDto>
                {
                    Success = true,
                    Data = student,
                    Message = "Student retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving student {id}", new { studentId = id });
            }
        }

        /// <summary>
        /// Get student progress metrics
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <returns>Student progress records</returns>
        [HttpGet("{id}/progress")]
        [ProducesResponseType(typeof(ApiResponseDto<List<StudentProgressDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<List<StudentProgressDto>>>> GetStudentProgress(int id)
        {
            try
            {
                // Check if student exists
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Student with ID {id} not found"
                    });
                }

                var progress = await _studentService.GetStudentProgressAsync(id);
                return Ok(new ApiResponseDto<List<StudentProgressDto>>
                {
                    Success = true,
                    Data = progress,
                    Message = "Student progress retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving progress for student {id}", 
                    new { studentId = id });
            }
        }

        /// <summary>
        /// Update student progress data
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <param name="progressDto">Progress data</param>
        /// <returns>Created progress record</returns>
        [HttpPost("{id}/progress")]
        [ProducesResponseType(typeof(ApiResponseDto<StudentProgressDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponseDto<StudentProgressDto>>> UpdateStudentProgress(
            int id, 
            [FromBody] StudentProgressCreateDto progressDto)
        {
            try
            {
                // Validate that the student ID in the URL matches the DTO
                if (progressDto.StudentId != id)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Student ID in URL does not match the progress data",
                        Errors = new List<string> { "Student ID mismatch" }
                    });
                }

                // Check if student exists
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Student with ID {id} not found"
                    });
                }

                var progress = await _studentService.CreateStudentProgressAsync(progressDto);
                return CreatedAtAction(
                    nameof(GetStudentProgress),
                    new { id = id },
                    new ApiResponseDto<StudentProgressDto>
                    {
                        Success = true,
                        Data = progress,
                        Message = "Student progress updated successfully"
                    });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error updating progress for student {id}", 
                    new { studentId = id, progressData = progressDto });
            }
        }

        /// <summary>
        /// Create a new student
        /// </summary>
        /// <param name="createDto">Student data</param>
        /// <returns>Created student</returns>
        [HttpPost]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(ApiResponseDto<StudentDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<StudentDto>>> CreateStudent([FromBody] StudentCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid student data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var student = await _studentService.CreateStudentAsync(createDto);
                return CreatedAtAction(
                    nameof(GetStudent),
                    new { id = student.Id },
                    new ApiResponseDto<StudentDto>
                    {
                        Success = true,
                        Data = student,
                        Message = "Student created successfully"
                    });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error creating student", 
                    new { studentData = createDto });
            }
        }

        /// <summary>
        /// Update a student
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <param name="updateDto">Updated student data</param>
        /// <returns>Updated student</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(ApiResponseDto<StudentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<StudentDto>>> UpdateStudent(
            int id, 
            [FromBody] StudentUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid student data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var student = await _studentService.UpdateStudentAsync(id, updateDto);
                if (student == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Student with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<StudentDto>
                {
                    Success = true,
                    Data = student,
                    Message = "Student updated successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error updating student {id}", 
                    new { studentId = id, updateData = updateDto });
            }
        }

        /// <summary>
        /// Delete a student
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteStudent(int id)
        {
            try
            {
                var result = await _studentService.DeleteStudentAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Student with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Student deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error deleting student {id}", 
                    new { studentId = id });
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
