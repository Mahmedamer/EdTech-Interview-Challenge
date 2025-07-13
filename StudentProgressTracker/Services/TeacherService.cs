using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services.Interfaces;

namespace StudentProgressTracker.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<TeacherService> _logger;
        private readonly UserManager<User> _userManager;

        public TeacherService(
            ApplicationDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<TeacherService> logger,
            UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<PaginatedResponseDto<TeacherDto>> GetTeachersAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            bool? isActive = null)
        {
            try
            {
                var query = _context.Users
                    .Where(u => u.Role == "Teacher")
                    .Include(u => u.TeacherStudents.Where(ts => ts.IsActive))
                        .ThenInclude(ts => ts.Student)
                            .ThenInclude(s => s.ProgressRecords)
                    .AsQueryable();

                // Apply filters
                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(searchLower) ||
                        u.LastName.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower));
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "name" => ascending ? query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                                       : query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName),
                    "email" => ascending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                    "students" => ascending ? query.OrderBy(u => u.TeacherStudents.Count(ts => ts.IsActive))
                                           : query.OrderByDescending(u => u.TeacherStudents.Count(ts => ts.IsActive)),
                    "lastlogin" => ascending ? query.OrderBy(u => u.LastLoginDate) : query.OrderByDescending(u => u.LastLoginDate),
                    "created" => ascending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                    _ => query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                };

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var teachers = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var teacherDtos = teachers.Select(CreateTeacherDto).ToList();

                return new PaginatedResponseDto<TeacherDto>
                {
                    Data = teacherDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                    Metadata = new Dictionary<string, object>
                    {
                        ["filters"] = new { searchTerm, sortBy, ascending, isActive },
                        ["totalActive"] = teacherDtos.Count(t => t.IsActive),
                        ["totalInactive"] = teacherDtos.Count(t => !t.IsActive),
                        ["averageStudentsPerTeacher"] = teacherDtos.Any() ? teacherDtos.Average(t => t.TotalStudents) : 0
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teachers with filters: SearchTerm={SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<TeacherDto?> GetTeacherByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"teacher_{id}";
                var cached = await _cacheService.GetAsync<TeacherDto>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var teacher = await _context.Users
                    .Where(u => u.Id == id && u.Role == "Teacher")
                    .Include(u => u.TeacherStudents.Where(ts => ts.IsActive))
                        .ThenInclude(ts => ts.Student)
                            .ThenInclude(s => s.ProgressRecords)
                    .FirstOrDefaultAsync();

                if (teacher == null) return null;

                var dto = CreateTeacherDto(teacher);
                await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher with ID {TeacherId}", id);
                throw;
            }
        }

        public async Task<TeacherDetailsDto?> GetTeacherDetailsAsync(int id)
        {
            try
            {
                var teacher = await _context.Users
                    .Where(u => u.Id == id && u.Role == "Teacher")
                    .Include(u => u.TeacherStudents)
                        .ThenInclude(ts => ts.Student)
                            .ThenInclude(s => s.ProgressRecords)
                                .ThenInclude(pr => pr.Subject)
                    .FirstOrDefaultAsync();

                if (teacher == null) return null;

                var detailsDto = _mapper.Map<TeacherDetailsDto>(teacher);
                
                // Map basic teacher info
                var basicDto = CreateTeacherDto(teacher);
                detailsDto.Id = basicDto.Id;
                detailsDto.FirstName = basicDto.FirstName;
                detailsDto.LastName = basicDto.LastName;
                detailsDto.FullName = basicDto.FullName;
                detailsDto.Email = basicDto.Email;
                detailsDto.Role = basicDto.Role;
                detailsDto.IsActive = basicDto.IsActive;
                detailsDto.LastLoginDate = basicDto.LastLoginDate;
                detailsDto.CreatedAt = basicDto.CreatedAt;
                detailsDto.UpdatedAt = basicDto.UpdatedAt;
                detailsDto.TotalStudents = basicDto.TotalStudents;
                detailsDto.ActiveStudents = basicDto.ActiveStudents;
                detailsDto.AssignedGrades = basicDto.AssignedGrades;
                detailsDto.Statistics = basicDto.Statistics;

                // Map assigned students
                detailsDto.AssignedStudents = teacher.TeacherStudents
                    .Where(ts => ts.IsActive)
                    .Select(ts => _mapper.Map<StudentDto>(ts.Student))
                    .ToList();

                // Map student assignments
                detailsDto.StudentAssignments = teacher.TeacherStudents
                    .Select(ts => new TeacherStudentAssignmentDto
                    {
                        Id = ts.Id,
                        TeacherId = ts.TeacherId,
                        StudentId = ts.StudentId,
                        StudentName = $"{ts.Student.FirstName} {ts.Student.LastName}",
                        StudentEmail = ts.Student.Email,
                        StudentGrade = ts.Student.Grade,
                        AssignedDate = ts.AssignedDate,
                        UnassignedDate = ts.UnassignedDate,
                        IsActive = ts.IsActive,
                        StudentProgress = ts.Student.ProgressRecords?.Any() == true
                            ? ts.Student.ProgressRecords.Average(pr => pr.CompletionPercentage)
                            : 0,
                        ProgressLevel = DetermineProgressLevel(ts.Student.ProgressRecords?.ToList() ?? new List<StudentProgress>())
                    }).ToList();

                // Calculate performance metrics
                detailsDto.Performance = await CalculateTeacherPerformanceAsync(teacher);

                return detailsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher details for ID {TeacherId}", id);
                throw;
            }
        }

        public async Task<TeacherDto> CreateTeacherAsync(TeacherCreateDto createDto)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(createDto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email '{createDto.Email}' already exists");
                }

                var teacher = _mapper.Map<User>(createDto);
                teacher.Role = "Teacher";
                teacher.UserName = createDto.Email;
                teacher.CreatedAt = DateTime.UtcNow;
                teacher.UpdatedAt = DateTime.UtcNow;
                teacher.IsActive = true;

                var result = await _userManager.CreateAsync(teacher, createDto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create teacher: {errors}");
                }

                _logger.LogInformation("Created new teacher: {TeacherName} ({TeacherEmail})", 
                    $"{teacher.FirstName} {teacher.LastName}", teacher.Email);

                // Clear related cache
                await _cacheService.RemovePatternAsync("teachers_*");

                return CreateTeacherDto(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher: {TeacherEmail}", createDto.Email);
                throw;
            }
        }

        public async Task<TeacherDto?> UpdateTeacherAsync(int id, TeacherUpdateDto updateDto)
        {
            try
            {
                var teacher = await _context.Users
                    .Where(u => u.Id == id && u.Role == "Teacher")
                    .Include(u => u.TeacherStudents.Where(ts => ts.IsActive))
                        .ThenInclude(ts => ts.Student)
                            .ThenInclude(s => s.ProgressRecords)
                    .FirstOrDefaultAsync();

                if (teacher == null) return null;

                // Check for duplicate email if email is being updated
                if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != teacher.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                    if (existingUser != null)
                    {
                        throw new InvalidOperationException($"User with email '{updateDto.Email}' already exists");
                    }
                }

                // Update properties
                if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                    teacher.FirstName = updateDto.FirstName;
                if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                    teacher.LastName = updateDto.LastName;
                if (!string.IsNullOrWhiteSpace(updateDto.Email))
                {
                    teacher.Email = updateDto.Email;
                    teacher.UserName = updateDto.Email;
                }
                if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
                    teacher.PhoneNumber = updateDto.PhoneNumber;
                if (updateDto.IsActive.HasValue)
                    teacher.IsActive = updateDto.IsActive.Value;

                teacher.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(teacher);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to update teacher: {errors}");
                }

                _logger.LogInformation("Updated teacher: {TeacherId} - {TeacherName}", id, $"{teacher.FirstName} {teacher.LastName}");

                // Clear cache
                await _cacheService.RemoveAsync($"teacher_{id}");
                await _cacheService.RemovePatternAsync("teachers_*");

                return CreateTeacherDto(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher with ID {TeacherId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            try
            {
                var teacher = await _context.Users
                    .Where(u => u.Id == id && u.Role == "Teacher")
                    .Include(u => u.TeacherStudents)
                    .FirstOrDefaultAsync();

                if (teacher == null) return false;

                // Check if teacher has active student assignments
                if (teacher.TeacherStudents?.Any(ts => ts.IsActive) == true)
                {
                    // Soft delete - just mark as inactive and unassign students
                    teacher.IsActive = false;
                    teacher.UpdatedAt = DateTime.UtcNow;

                    // Unassign all students
                    foreach (var assignment in teacher.TeacherStudents.Where(ts => ts.IsActive))
                    {
                        assignment.IsActive = false;
                        assignment.UnassignedDate = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Soft deleted teacher: {TeacherId} - {TeacherName} (had active assignments)", 
                        id, $"{teacher.FirstName} {teacher.LastName}");
                }
                else
                {
                    // Hard delete if no active assignments
                    var result = await _userManager.DeleteAsync(teacher);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to delete teacher: {errors}");
                    }
                    _logger.LogInformation("Hard deleted teacher: {TeacherId} - {TeacherName}", 
                        id, $"{teacher.FirstName} {teacher.LastName}");
                }

                // Clear cache
                await _cacheService.RemoveAsync($"teacher_{id}");
                await _cacheService.RemovePatternAsync("teachers_*");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting teacher with ID {TeacherId}", id);
                throw;
            }
        }

        public async Task<List<TeacherDto>> GetActiveTeachersAsync()
        {
            try
            {
                var cacheKey = "active_teachers";
                var cached = await _cacheService.GetAsync<List<TeacherDto>>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var teachers = await _context.Users
                    .Where(u => u.Role == "Teacher" && u.IsActive)
                    .Include(u => u.TeacherStudents.Where(ts => ts.IsActive))
                        .ThenInclude(ts => ts.Student)
                            .ThenInclude(s => s.ProgressRecords)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                var teacherDtos = teachers.Select(CreateTeacherDto).ToList();
                await _cacheService.SetAsync(cacheKey, teacherDtos, TimeSpan.FromHours(1));
                return teacherDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active teachers");
                throw;
            }
        }

        public async Task<TeacherStatisticsDto> GetTeacherStatisticsAsync(int id)
        {
            try
            {
                var teacher = await _context.Users
                    .Where(u => u.Id == id && u.Role == "Teacher")
                    .Include(u => u.TeacherStudents)
                        .ThenInclude(ts => ts.Student)
                            .ThenInclude(s => s.ProgressRecords)
                    .FirstOrDefaultAsync();

                if (teacher == null)
                {
                    throw new InvalidOperationException($"Teacher with ID {id} not found");
                }

                var allStudents = teacher.TeacherStudents.Select(ts => ts.Student).ToList();
                var activeStudents = allStudents.Where(s => s.IsActive).ToList();
                var allProgressRecords = allStudents.SelectMany(s => s.ProgressRecords ?? new List<StudentProgress>()).ToList();

                var statistics = new TeacherStatisticsDto
                {
                    TotalStudents = allStudents.Count,
                    ActiveStudents = activeStudents.Count,
                    InactiveStudents = allStudents.Count - activeStudents.Count,
                    AverageStudentProgress = allProgressRecords.Any() ? allProgressRecords.Average(pr => pr.CompletionPercentage) : 0,
                    AverageStudentPerformance = allProgressRecords.Any() ? allProgressRecords.Average(pr => pr.PerformanceScore) : 0,
                    LastActivityDate = allProgressRecords.Any() ? allProgressRecords.Max(pr => pr.LastActivityDate) : null
                };

                // Calculate performance level distribution
                var performanceLevels = new Dictionary<string, int>
                {
                    ["Struggling"] = 0,
                    ["OnTrack"] = 0,
                    ["Advanced"] = 0
                };

                foreach (var student in activeStudents)
                {
                    var studentProgress = student.ProgressRecords?.ToList() ?? new List<StudentProgress>();
                    var level = DetermineProgressLevel(studentProgress);
                    if (performanceLevels.ContainsKey(level))
                        performanceLevels[level]++;
                }

                statistics.StudentsStruggling = performanceLevels["Struggling"];
                statistics.StudentsOnTrack = performanceLevels["OnTrack"];
                statistics.StudentsAdvanced = performanceLevels["Advanced"];

                // Calculate grade distribution
                statistics.GradeDistribution = activeStudents
                    .GroupBy(s => s.Grade)
                    .Select(g => new GradeDistributionDto
                    {
                        Grade = g.Key,
                        StudentCount = g.Count(),
                        AverageProgress = g.SelectMany(s => s.ProgressRecords ?? new List<StudentProgress>())
                                         .Any() ? g.SelectMany(s => s.ProgressRecords ?? new List<StudentProgress>())
                                                   .Average(pr => pr.CompletionPercentage) : 0
                    }).ToList();

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating statistics for teacher ID {TeacherId}", id);
                throw;
            }
        }

        public async Task<List<StudentDto>> GetTeacherStudentsAsync(int teacherId)
        {
            try
            {
                var teacherStudents = await _context.TeacherStudents
                    .Where(ts => ts.TeacherId == teacherId && ts.IsActive)
                    .Include(ts => ts.Student)
                        .ThenInclude(s => s.ProgressRecords)
                    .Select(ts => ts.Student)
                    .ToListAsync();

                return teacherStudents.Select(s => _mapper.Map<StudentDto>(s)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students for teacher ID {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<TeacherStudentAssignmentDto> AssignStudentToTeacherAsync(TeacherStudentAssignmentCreateDto assignmentDto)
        {
            try
            {
                // Check if assignment already exists
                var existingAssignment = await _context.TeacherStudents
                    .FirstOrDefaultAsync(ts => ts.TeacherId == assignmentDto.TeacherId && 
                                             ts.StudentId == assignmentDto.StudentId && 
                                             ts.IsActive);

                if (existingAssignment != null)
                {
                    throw new InvalidOperationException("Student is already assigned to this teacher");
                }

                var assignment = _mapper.Map<TeacherStudent>(assignmentDto);
                assignment.CreatedAt = DateTime.UtcNow;
                assignment.IsActive = true;

                _context.TeacherStudents.Add(assignment);
                await _context.SaveChangesAsync();

                // Load related data for response
                var fullAssignment = await _context.TeacherStudents
                    .Include(ts => ts.Student)
                        .ThenInclude(s => s.ProgressRecords)
                    .FirstAsync(ts => ts.Id == assignment.Id);

                _logger.LogInformation("Assigned student {StudentId} to teacher {TeacherId}", 
                    assignmentDto.StudentId, assignmentDto.TeacherId);

                // Clear cache
                await _cacheService.RemovePatternAsync("teacher_*");
                await _cacheService.RemovePatternAsync("teachers_*");

                return new TeacherStudentAssignmentDto
                {
                    Id = fullAssignment.Id,
                    TeacherId = fullAssignment.TeacherId,
                    StudentId = fullAssignment.StudentId,
                    StudentName = $"{fullAssignment.Student.FirstName} {fullAssignment.Student.LastName}",
                    StudentEmail = fullAssignment.Student.Email,
                    StudentGrade = fullAssignment.Student.Grade,
                    AssignedDate = fullAssignment.AssignedDate,
                    UnassignedDate = fullAssignment.UnassignedDate,
                    IsActive = fullAssignment.IsActive,
                    StudentProgress = fullAssignment.Student.ProgressRecords?.Any() == true
                        ? fullAssignment.Student.ProgressRecords.Average(pr => pr.CompletionPercentage)
                        : 0,
                    ProgressLevel = DetermineProgressLevel(fullAssignment.Student.ProgressRecords?.ToList() ?? new List<StudentProgress>())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning student {StudentId} to teacher {TeacherId}", 
                    assignmentDto.StudentId, assignmentDto.TeacherId);
                throw;
            }
        }

        public async Task<bool> UnassignStudentFromTeacherAsync(int teacherId, int studentId)
        {
            try
            {
                var assignment = await _context.TeacherStudents
                    .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && 
                                             ts.StudentId == studentId && 
                                             ts.IsActive);

                if (assignment == null) return false;

                assignment.IsActive = false;
                assignment.UnassignedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Unassigned student {StudentId} from teacher {TeacherId}", studentId, teacherId);

                // Clear cache
                await _cacheService.RemovePatternAsync("teacher_*");
                await _cacheService.RemovePatternAsync("teachers_*");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning student {StudentId} from teacher {TeacherId}", studentId, teacherId);
                throw;
            }
        }

        public async Task<List<TeacherStudentAssignmentDto>> BulkAssignStudentsToTeacherAsync(BulkTeacherStudentAssignmentDto bulkAssignmentDto)
        {
            try
            {
                var results = new List<TeacherStudentAssignmentDto>();

                foreach (var studentId in bulkAssignmentDto.StudentIds)
                {
                    try
                    {
                        var assignmentDto = new TeacherStudentAssignmentCreateDto
                        {
                            TeacherId = bulkAssignmentDto.TeacherId,
                            StudentId = studentId,
                            AssignedDate = bulkAssignmentDto.AssignedDate
                        };

                        var result = await AssignStudentToTeacherAsync(assignmentDto);
                        results.Add(result);
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("already assigned"))
                    {
                        _logger.LogWarning("Student {StudentId} is already assigned to teacher {TeacherId}", 
                            studentId, bulkAssignmentDto.TeacherId);
                        // Continue with other students
                    }
                }

                _logger.LogInformation("Bulk assigned {Count} students to teacher {TeacherId}", 
                    results.Count, bulkAssignmentDto.TeacherId);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning students to teacher {TeacherId}", bulkAssignmentDto.TeacherId);
                throw;
            }
        }

        public async Task<bool> UpdateTeacherStudentAssignmentAsync(int assignmentId, TeacherStudentAssignmentUpdateDto updateDto)
        {
            try
            {
                var assignment = await _context.TeacherStudents.FindAsync(assignmentId);
                if (assignment == null) return false;

                if (updateDto.UnassignedDate.HasValue)
                    assignment.UnassignedDate = updateDto.UnassignedDate.Value;
                if (updateDto.IsActive.HasValue)
                    assignment.IsActive = updateDto.IsActive.Value;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated teacher-student assignment {AssignmentId}", assignmentId);

                // Clear cache
                await _cacheService.RemovePatternAsync("teacher_*");
                await _cacheService.RemovePatternAsync("teachers_*");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher-student assignment {AssignmentId}", assignmentId);
                throw;
            }
        }

        #region Private Helper Methods

        private TeacherDto CreateTeacherDto(User teacher)
        {
            var activeStudents = teacher.TeacherStudents?.Where(ts => ts.IsActive).ToList() ?? new List<TeacherStudent>();
            var allProgressRecords = activeStudents
                .SelectMany(ts => ts.Student.ProgressRecords ?? new List<StudentProgress>())
                .ToList();

            return new TeacherDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                FullName = $"{teacher.FirstName} {teacher.LastName}",
                Email = teacher.Email ?? string.Empty,
                Role = teacher.Role,
                IsActive = teacher.IsActive,
                LastLoginDate = teacher.LastLoginDate,
                CreatedAt = teacher.CreatedAt,
                UpdatedAt = teacher.UpdatedAt,
                TotalStudents = activeStudents.Count,
                ActiveStudents = activeStudents.Count(ts => ts.Student.IsActive),
                AssignedGrades = activeStudents.Select(ts => ts.Student.Grade).Distinct().OrderBy(g => g).Select(g => g.ToString()).ToList(),
                Statistics = new TeacherStatisticsDto
                {
                    TotalStudents = activeStudents.Count,
                    ActiveStudents = activeStudents.Count(ts => ts.Student.IsActive),
                    InactiveStudents = activeStudents.Count(ts => !ts.Student.IsActive),
                    AverageStudentProgress = allProgressRecords.Any() ? allProgressRecords.Average(pr => pr.CompletionPercentage) : 0,
                    AverageStudentPerformance = allProgressRecords.Any() ? allProgressRecords.Average(pr => pr.PerformanceScore) : 0,
                    LastActivityDate = allProgressRecords.Any() ? allProgressRecords.Max(pr => pr.LastActivityDate) : null
                }
            };
        }

        private async Task<TeacherPerformanceDto> CalculateTeacherPerformanceAsync(User teacher)
        {
            var activeStudents = teacher.TeacherStudents?.Where(ts => ts.IsActive).Select(ts => ts.Student).ToList() ?? new List<Student>();
            var allProgressRecords = activeStudents.SelectMany(s => s.ProgressRecords ?? new List<StudentProgress>()).ToList();

            var performance = new TeacherPerformanceDto
            {
                OverallEffectiveness = allProgressRecords.Any() ? allProgressRecords.Average(pr => pr.PerformanceScore) : 0,
                StudentEngagement = allProgressRecords.Any() ? allProgressRecords.Average(pr => pr.CompletionPercentage) : 0,
                ProgressImprovement = 0, // Would need historical data for proper calculation
                CompletedAssignments = 0, // Would need assignment data
                PendingAssignments = 0, // Would need assignment data
                MonthlyTrends = new List<MonthlyPerformanceDto>()
            };

            // Calculate monthly trends for the last 6 months
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyData = allProgressRecords
                .Where(pr => pr.AssessmentDate >= sixMonthsAgo)
                .GroupBy(pr => new { pr.AssessmentDate.Year, pr.AssessmentDate.Month })
                .Select(g => new MonthlyPerformanceDto
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    AverageProgress = g.Average(pr => pr.CompletionPercentage),
                    AveragePerformance = g.Average(pr => pr.PerformanceScore),
                    ActiveStudents = g.Select(pr => pr.StudentId).Distinct().Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            performance.MonthlyTrends = monthlyData;

            return performance;
        }

        private string DetermineProgressLevel(List<StudentProgress> progressRecords)
        {
            if (!progressRecords.Any()) return "New";

            var averagePerformance = progressRecords.Average(p => p.PerformanceScore);
            var averageCompletion = progressRecords.Average(p => p.CompletionPercentage);

            if (averagePerformance >= 85 && averageCompletion >= 90)
                return "Advanced";
            else if (averagePerformance >= 70 && averageCompletion >= 70)
                return "OnTrack";
            else
                return "Struggling";
        }

        #endregion
    }
}