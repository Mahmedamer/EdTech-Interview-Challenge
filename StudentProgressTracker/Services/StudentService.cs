using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services.Interfaces;

namespace StudentProgressTracker.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            ApplicationDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<StudentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PaginatedResponseDto<StudentDto>> GetStudentsAsync(
            int page = 1,
            int pageSize = 10,
            int? grade = null,
            string? subject = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true)
        {
            try
            {
                var query = _context.Students
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Subject)
                    .AsQueryable();

                // Apply filters
                if (grade.HasValue)
                {
                    query = query.Where(s => s.Grade == grade.Value);
                }

                if (!string.IsNullOrWhiteSpace(subject))
                {
                    query = query.Where(s => s.ProgressRecords
                        .Any(p => p.Subject.Name.Contains(subject) || p.Subject.Code.Contains(subject)));
                }

                if (startDate.HasValue || endDate.HasValue)
                {
                    query = query.Where(s => s.ProgressRecords
                        .Any(p => (!startDate.HasValue || p.AssessmentDate >= startDate.Value) &&
                                 (!endDate.HasValue || p.AssessmentDate <= endDate.Value)));
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(s => 
                        s.FirstName.ToLower().Contains(searchLower) ||
                        s.LastName.ToLower().Contains(searchLower) ||
                        s.Email.ToLower().Contains(searchLower));
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "name" => ascending ? query.OrderBy(s => s.FirstName).ThenBy(s => s.LastName) 
                                       : query.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName),
                    "progress" => ascending ? query.OrderBy(s => s.ProgressRecords.Average(p => p.CompletionPercentage))
                                           : query.OrderByDescending(s => s.ProgressRecords.Average(p => p.CompletionPercentage)),
                    "lastactivity" => ascending ? query.OrderBy(s => s.ProgressRecords.Max(p => p.LastActivityDate))
                                               : query.OrderByDescending(s => s.ProgressRecords.Max(p => p.LastActivityDate)),
                    _ => query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
                };

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var students = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var studentDtos = students.Select(student => new StudentDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    FullName = student.FullName,
                    Email = student.Email,
                    Grade = student.Grade,
                    DateOfBirth = student.DateOfBirth,
                    Age = student.Age,
                    EnrollmentDate = student.EnrollmentDate,
                    IsActive = student.IsActive,
                    Notes = student.Notes,
                    LastActivityDate = student.ProgressRecords.Any() 
                        ? student.ProgressRecords.Max(p => p.LastActivityDate) 
                        : student.CreatedAt,
                    OverallProgress = student.ProgressRecords.Any() 
                        ? student.ProgressRecords.Average(p => p.CompletionPercentage) 
                        : 0,
                    ProgressLevel = DetermineProgressLevel(student.ProgressRecords.ToList())
                }).ToList();

                return new PaginatedResponseDto<StudentDto>
                {
                    Data = studentDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                    Metadata = new Dictionary<string, object>
                    {
                        ["filters"] = new { grade, subject, startDate, endDate, searchTerm },
                        ["sorting"] = new { sortBy, ascending }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students with filters");
                throw;
            }
        }

        public async Task<StudentDto?> GetStudentByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"student_{id}";
                var cachedStudent = await _cacheService.GetAsync<StudentDto>(cacheKey);
                if (cachedStudent != null)
                {
                    return cachedStudent;
                }

                var student = await _context.Students
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Subject)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null) return null;

                var studentDto = new StudentDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    FullName = student.FullName,
                    Email = student.Email,
                    Grade = student.Grade,
                    DateOfBirth = student.DateOfBirth,
                    Age = student.Age,
                    EnrollmentDate = student.EnrollmentDate,
                    IsActive = student.IsActive,
                    Notes = student.Notes,
                    LastActivityDate = student.ProgressRecords.Any() 
                        ? student.ProgressRecords.Max(p => p.LastActivityDate) 
                        : student.CreatedAt,
                    OverallProgress = student.ProgressRecords.Any() 
                        ? student.ProgressRecords.Average(p => p.CompletionPercentage) 
                        : 0,
                    ProgressLevel = DetermineProgressLevel(student.ProgressRecords.ToList())
                };

                await _cacheService.SetAsync(cacheKey, studentDto, TimeSpan.FromMinutes(30));
                return studentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with ID {StudentId}", id);
                throw;
            }
        }

        public async Task<StudentDto> CreateStudentAsync(StudentCreateDto createDto)
        {
            try
            {
                var student = new Student
                {
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    Email = createDto.Email,
                    Grade = createDto.Grade,
                    DateOfBirth = createDto.DateOfBirth,
                    EnrollmentDate = createDto.EnrollmentDate,
                    Notes = createDto.Notes
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                var studentDto = _mapper.Map<StudentDto>(student);
                studentDto.OverallProgress = 0;
                studentDto.ProgressLevel = "New";

                _logger.LogInformation("Created new student with ID {StudentId}", student.Id);
                return studentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                throw;
            }
        }

        public async Task<StudentDto?> UpdateStudentAsync(int id, StudentUpdateDto updateDto)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null) return null;

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                    student.FirstName = updateDto.FirstName;
                
                if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                    student.LastName = updateDto.LastName;
                
                if (!string.IsNullOrWhiteSpace(updateDto.Email))
                    student.Email = updateDto.Email;
                
                if (updateDto.Grade.HasValue)
                    student.Grade = updateDto.Grade.Value;
                
                if (updateDto.DateOfBirth.HasValue)
                    student.DateOfBirth = updateDto.DateOfBirth.Value;
                
                if (updateDto.IsActive.HasValue)
                    student.IsActive = updateDto.IsActive.Value;
                
                if (updateDto.Notes != null)
                    student.Notes = updateDto.Notes;

                await _context.SaveChangesAsync();

                // Clear cache
                await _cacheService.RemoveAsync($"student_{id}");

                var updatedDto = _mapper.Map<StudentDto>(student);
                _logger.LogInformation("Updated student with ID {StudentId}", id);
                return updatedDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student with ID {StudentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null) return false;

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // Clear cache
                await _cacheService.RemoveAsync($"student_{id}");

                _logger.LogInformation("Deleted student with ID {StudentId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student with ID {StudentId}", id);
                throw;
            }
        }

        public async Task<List<StudentProgressDto>> GetStudentProgressAsync(int studentId)
        {
            try
            {
                var progressRecords = await _context.StudentProgress
                    .Include(p => p.Student)
                    .Include(p => p.Subject)
                    .Where(p => p.StudentId == studentId)
                    .OrderByDescending(p => p.AssessmentDate)
                    .ToListAsync();

                return progressRecords.Select(p => new StudentProgressDto
                {
                    Id = p.Id,
                    StudentId = p.StudentId,
                    StudentName = p.Student.FullName,
                    SubjectId = p.SubjectId,
                    SubjectName = p.Subject.Name,
                    CompletionPercentage = p.CompletionPercentage,
                    PerformanceScore = p.PerformanceScore,
                    TimeSpentMinutes = p.TimeSpentMinutes,
                    AssessmentDate = p.AssessmentDate,
                    ProgressLevel = p.ProgressLevel,
                    Notes = p.Notes,
                    LastActivityDate = p.LastActivityDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress for student {StudentId}", studentId);
                throw;
            }
        }

        public async Task<StudentProgressDto> CreateStudentProgressAsync(StudentProgressCreateDto progressDto)
        {
            try
            {
                var progress = new StudentProgress
                {
                    StudentId = progressDto.StudentId,
                    SubjectId = progressDto.SubjectId,
                    CompletionPercentage = progressDto.CompletionPercentage,
                    PerformanceScore = progressDto.PerformanceScore,
                    TimeSpentMinutes = progressDto.TimeSpentMinutes,
                    AssessmentDate = progressDto.AssessmentDate,
                    ProgressLevel = progressDto.ProgressLevel,
                    Notes = progressDto.Notes,
                    LastActivityDate = DateTime.UtcNow
                };

                _context.StudentProgress.Add(progress);
                await _context.SaveChangesAsync();

                // Clear student cache to update overall progress
                await _cacheService.RemoveAsync($"student_{progressDto.StudentId}");

                var student = await _context.Students.FindAsync(progressDto.StudentId);
                var subject = await _context.Subjects.FindAsync(progressDto.SubjectId);

                var result = new StudentProgressDto
                {
                    Id = progress.Id,
                    StudentId = progress.StudentId,
                    StudentName = student?.FullName ?? "",
                    SubjectId = progress.SubjectId,
                    SubjectName = subject?.Name ?? "",
                    CompletionPercentage = progress.CompletionPercentage,
                    PerformanceScore = progress.PerformanceScore,
                    TimeSpentMinutes = progress.TimeSpentMinutes,
                    AssessmentDate = progress.AssessmentDate,
                    ProgressLevel = progress.ProgressLevel,
                    Notes = progress.Notes,
                    LastActivityDate = progress.LastActivityDate
                };

                _logger.LogInformation("Created progress record {ProgressId} for student {StudentId}", 
                    progress.Id, progressDto.StudentId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating progress record for student {StudentId}", progressDto.StudentId);
                throw;
            }
        }

        public async Task<List<StudentDto>> GetStudentsByTeacherAsync(int teacherId)
        {
            try
            {
                var students = await _context.TeacherStudents
                    .Include(ts => ts.Student)
                        .ThenInclude(s => s.ProgressRecords)
                            .ThenInclude(p => p.Subject)
                    .Where(ts => ts.TeacherId == teacherId && ts.IsActive)
                    .Select(ts => ts.Student)
                    .ToListAsync();

                return students.Select(student => new StudentDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    FullName = student.FullName,
                    Email = student.Email,
                    Grade = student.Grade,
                    DateOfBirth = student.DateOfBirth,
                    Age = student.Age,
                    EnrollmentDate = student.EnrollmentDate,
                    IsActive = student.IsActive,
                    Notes = student.Notes,
                    LastActivityDate = student.ProgressRecords.Any() 
                        ? student.ProgressRecords.Max(p => p.LastActivityDate) 
                        : student.CreatedAt,
                    OverallProgress = student.ProgressRecords.Any() 
                        ? student.ProgressRecords.Average(p => p.CompletionPercentage) 
                        : 0,
                    ProgressLevel = DetermineProgressLevel(student.ProgressRecords.ToList())
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students for teacher {TeacherId}", teacherId);
                throw;
            }
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
    }
}
