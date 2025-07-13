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
    [Authorize(Policy = "AdminOnly")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(ITeacherService teacherService, ILogger<TeachersController> logger)
        {
            _teacherService = teacherService;
            _logger = logger;
        }

        /// <summary>
        /// Get all teachers with filtering, pagination, and sorting (Admin Only)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="searchTerm">Search in name or email</param>
        /// <param name="sortBy">Sort by: name, email, students, lastlogin, created</param>
        /// <param name="ascending">Sort direction (default: true)</param>
        /// <param name="isActive">Filter by active status</param>
        /// <returns>Paginated list of teachers</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResponseDto<TeacherDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<TeacherDto>>>> GetTeachers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
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

                var result = await _teacherService.GetTeachersAsync(
                    page, pageSize, searchTerm, sortBy, ascending, isActive);

                return Ok(new ApiResponseDto<PaginatedResponseDto<TeacherDto>>
                {
                    Success = true,
                    Data = result,
                    Message = $"Retrieved {result.Data.Count} teachers (page {page} of {result.TotalPages})",
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
                        ["filters"] = new { searchTerm, sortBy, ascending, isActive }
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error retrieving teachers", 
                    new { page, pageSize, searchTerm, sortBy, ascending, isActive });
            }
        }

        /// <summary>
        /// Get active teachers list (Admin Only)
        /// </summary>
        /// <returns>List of active teachers</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponseDto<List<TeacherDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<List<TeacherDto>>>> GetActiveTeachers()
        {
            try
            {
                var teachers = await _teacherService.GetActiveTeachersAsync();

                return Ok(new ApiResponseDto<List<TeacherDto>>
                {
                    Success = true,
                    Data = teachers,
                    Message = $"Retrieved {teachers.Count} active teachers",
                    Metadata = new Dictionary<string, object>
                    {
                        ["totalActive"] = teachers.Count,
                        ["totalStudents"] = teachers.Sum(t => t.TotalStudents),
                        ["averageStudentsPerTeacher"] = teachers.Any() ? teachers.Average(t => t.TotalStudents) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error retrieving active teachers");
            }
        }

        /// <summary>
        /// Get a specific teacher by ID (Admin Only)
        /// </summary>
        /// <param name="id">Teacher ID</param>
        /// <returns>Teacher details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<TeacherDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<TeacherDto>>> GetTeacher(int id)
        {
            try
            {
                var teacher = await _teacherService.GetTeacherByIdAsync(id);

                if (teacher == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Teacher with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<TeacherDto>
                {
                    Success = true,
                    Data = teacher,
                    Message = "Teacher retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving teacher {id}", 
                    new { teacherId = id });
            }
        }

        /// <summary>
        /// Get detailed teacher information including students and performance (Admin Only)
        /// </summary>
        /// <param name="id">Teacher ID</param>
        /// <returns>Detailed teacher information</returns>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponseDto<TeacherDetailsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<TeacherDetailsDto>>> GetTeacherDetails(int id)
        {
            try
            {
                var teacherDetails = await _teacherService.GetTeacherDetailsAsync(id);

                if (teacherDetails == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Teacher with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<TeacherDetailsDto>
                {
                    Success = true,
                    Data = teacherDetails,
                    Message = "Teacher details retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["assignedStudentsCount"] = teacherDetails.AssignedStudents.Count,
                        ["totalAssignmentsCount"] = teacherDetails.StudentAssignments.Count,
                        ["performanceIncluded"] = true
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error retrieving teacher details for {id}", 
                    new { teacherId = id });
            }
        }

        /// <summary>
        /// Get teacher statistics and performance metrics (Admin Only)
        /// </summary>
        /// <param name="id">Teacher ID</param>
        /// <returns>Teacher statistics</returns>
        [HttpGet("{id}/statistics")]
        [ProducesResponseType(typeof(ApiResponseDto<TeacherStatisticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<TeacherStatisticsDto>>> GetTeacherStatistics(int id)
        {
            try
            {
                var statistics = await _teacherService.GetTeacherStatisticsAsync(id);

                return Ok(new ApiResponseDto<TeacherStatisticsDto>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Teacher statistics retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["generatedAt"] = DateTime.UtcNow,
                        ["teacherId"] = id
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
                return HandleDatabaseException(ex, $"Error retrieving statistics for teacher {id}", 
                    new { teacherId = id });
            }
        }

        /// <summary>
        /// Get students assigned to a specific teacher (Admin Only)
        /// </summary>
        /// <param name="id">Teacher ID</param>
        /// <returns>List of students assigned to the teacher</returns>
        [HttpGet("{id}/students")]
        [ProducesResponseType(typeof(ApiResponseDto<List<StudentDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<List<StudentDto>>>> GetTeacherStudents(int id)
        {
            try
            {
                // Verify teacher exists
                var teacher = await _teacherService.GetTeacherByIdAsync(id);
                if (teacher == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Teacher with ID {id} not found"
                    });
                }

                var students = await _teacherService.GetTeacherStudentsAsync(id);
                return Ok(new ApiResponseDto<List<StudentDto>>
                {
                    Success = true,
                    Data = students,
                    Message = "Teacher students retrieved successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["teacherId"] = id,
                        ["teacherName"] = teacher.FullName,
                        ["studentCount"] = students.Count,
                        ["averageProgress"] = students.Any() ? students.Average(s => s.OverallProgress) : 0
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
        /// Create a new teacher account (Admin Only)
        /// </summary>
        /// <param name="createDto">Teacher creation data</param>
        /// <returns>Created teacher</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<TeacherDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<TeacherDto>>> CreateTeacher([FromBody] TeacherCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid teacher data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var teacher = await _teacherService.CreateTeacherAsync(createDto);

                return CreatedAtAction(
                    nameof(GetTeacher),
                    new { id = teacher.Id },
                    new ApiResponseDto<TeacherDto>
                    {
                        Success = true,
                        Data = teacher,
                        Message = $"Teacher '{teacher.FullName}' created successfully"
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
                return HandleDatabaseException(ex, "Error creating teacher", 
                    new { teacherData = createDto });
            }
        }

        /// <summary>
        /// Update an existing teacher (Admin Only)
        /// </summary>
        /// <param name="id">Teacher ID</param>
        /// <param name="updateDto">Teacher update data</param>
        /// <returns>Updated teacher</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<TeacherDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<TeacherDto>>> UpdateTeacher(int id, [FromBody] TeacherUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid teacher data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var teacher = await _teacherService.UpdateTeacherAsync(id, updateDto);

                if (teacher == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Teacher with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<TeacherDto>
                {
                    Success = true,
                    Data = teacher,
                    Message = $"Teacher '{teacher.FullName}' updated successfully"
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
                return HandleDatabaseException(ex, $"Error updating teacher {id}", 
                    new { teacherId = id, updateData = updateDto });
            }
        }

        /// <summary>
        /// Delete a teacher (Admin Only) - Soft delete if has active assignments
        /// </summary>
        /// <param name="id">Teacher ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteTeacher(int id)
        {
            try
            {
                var result = await _teacherService.DeleteTeacherAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Teacher with ID {id} not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Teacher deleted successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["deletedId"] = id,
                        ["deletedAt"] = DateTime.UtcNow,
                        ["note"] = "Teacher may have been soft-deleted if they had active student assignments"
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, $"Error deleting teacher {id}", 
                    new { teacherId = id });
            }
        }

        /// <summary>
        /// Assign a student to a teacher (Admin Only)
        /// </summary>
        /// <param name="assignmentDto">Student-teacher assignment data</param>
        /// <returns>Created assignment</returns>
        [HttpPost("assignments")]
        [ProducesResponseType(typeof(ApiResponseDto<TeacherStudentAssignmentDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<TeacherStudentAssignmentDto>>> AssignStudentToTeacher(
            [FromBody] TeacherStudentAssignmentCreateDto assignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid assignment data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var assignment = await _teacherService.AssignStudentToTeacherAsync(assignmentDto);

                return CreatedAtAction(
                    nameof(GetTeacherStudents),
                    new { id = assignment.TeacherId },
                    new ApiResponseDto<TeacherStudentAssignmentDto>
                    {
                        Success = true,
                        Data = assignment,
                        Message = $"Student '{assignment.StudentName}' assigned to teacher successfully"
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
                return HandleDatabaseException(ex, "Error assigning student to teacher", 
                    new { assignmentData = assignmentDto });
            }
        }

        /// <summary>
        /// Bulk assign multiple students to a teacher (Admin Only)
        /// </summary>
        /// <param name="bulkAssignmentDto">Bulk assignment data</param>
        /// <returns>List of created assignments</returns>
        [HttpPost("assignments/bulk")]
        [ProducesResponseType(typeof(ApiResponseDto<List<TeacherStudentAssignmentDto>>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<List<TeacherStudentAssignmentDto>>>> BulkAssignStudentsToTeacher(
            [FromBody] BulkTeacherStudentAssignmentDto bulkAssignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid bulk assignment data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var assignments = await _teacherService.BulkAssignStudentsToTeacherAsync(bulkAssignmentDto);

                return CreatedAtAction(
                    nameof(GetTeacherStudents),
                    new { id = bulkAssignmentDto.TeacherId },
                    new ApiResponseDto<List<TeacherStudentAssignmentDto>>
                    {
                        Success = true,
                        Data = assignments,
                        Message = $"Successfully assigned {assignments.Count} students to teacher",
                        Metadata = new Dictionary<string, object>
                        {
                            ["totalRequested"] = bulkAssignmentDto.StudentIds.Count,
                            ["successfulAssignments"] = assignments.Count,
                            ["skippedDuplicates"] = bulkAssignmentDto.StudentIds.Count - assignments.Count
                        }
                    });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error bulk assigning students to teacher", 
                    new { bulkAssignmentData = bulkAssignmentDto });
            }
        }

        /// <summary>
        /// Unassign a student from a teacher (Admin Only)
        /// </summary>
        /// <param name="teacherId">Teacher ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>Unassignment result</returns>
        [HttpDelete("{teacherId}/students/{studentId}")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 503)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApiResponseDto<object>>> UnassignStudentFromTeacher(int teacherId, int studentId)
        {
            try
            {
                var result = await _teacherService.UnassignStudentFromTeacherAsync(teacherId, studentId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = $"Active assignment between teacher {teacherId} and student {studentId} not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Student unassigned from teacher successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["teacherId"] = teacherId,
                        ["studentId"] = studentId,
                        ["unassignedAt"] = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleDatabaseException(ex, "Error unassigning student from teacher", 
                    new { teacherId, studentId });
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