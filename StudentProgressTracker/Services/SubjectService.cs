using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services.Interfaces;

namespace StudentProgressTracker.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SubjectService> _logger;

        public SubjectService(
            ApplicationDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<SubjectService> logger)
        {
            _context = context;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PaginatedResponseDto<SubjectDto>> GetSubjectsAsync(
            int page = 1,
            int pageSize = 10,
            int? grade = null,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            bool? isActive = null)
        {
            try
            {
                var query = _context.Subjects
                    .Include(s => s.Assignments)
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Student)
                    .AsQueryable();

                // Apply filters
                if (grade.HasValue)
                {
                    query = query.Where(s => s.Grade == grade.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(s =>
                        s.Name.ToLower().Contains(searchLower) ||
                        s.Code.ToLower().Contains(searchLower) ||
                        (s.Description != null && s.Description.ToLower().Contains(searchLower)));
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "name" => ascending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                    "code" => ascending ? query.OrderBy(s => s.Code) : query.OrderByDescending(s => s.Code),
                    "grade" => ascending ? query.OrderBy(s => s.Grade) : query.OrderByDescending(s => s.Grade),
                    "createdate" => ascending ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                    "students" => ascending ? query.OrderBy(s => s.ProgressRecords.Select(p => p.StudentId).Distinct().Count())
                                           : query.OrderByDescending(s => s.ProgressRecords.Select(p => p.StudentId).Distinct().Count()),
                    _ => query.OrderBy(s => s.Name)
                };

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var subjects = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var subjectDtos = subjects.Select(s => 
                {
                    var dto = _mapper.Map<SubjectDto>(s);
                    dto.TotalAssignments = s.Assignments?.Count ?? 0;
                    dto.ActiveStudents = s.ProgressRecords?
                        .Where(p => p.Student.IsActive)
                        .Select(p => p.StudentId)
                        .Distinct()
                        .Count() ?? 0;
                    dto.AverageProgress = s.ProgressRecords?.Any() == true
                        ? s.ProgressRecords.Average(p => p.CompletionPercentage)
                        : 0;
                    return dto;
                }).ToList();

                return new PaginatedResponseDto<SubjectDto>
                {
                    Data = subjectDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                    Metadata = new Dictionary<string, object>
                    {
                        ["filters"] = new { grade, searchTerm, sortBy, ascending, isActive },
                        ["totalActive"] = subjectDtos.Count(s => s.IsActive),
                        ["totalInactive"] = subjectDtos.Count(s => !s.IsActive)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subjects with filters: Grade={Grade}, SearchTerm={SearchTerm}", grade, searchTerm);
                throw;
            }
        }

        public async Task<SubjectDto?> GetSubjectByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"subject_{id}";
                var cached = await _cacheService.GetAsync<SubjectDto>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var subject = await _context.Subjects
                    .Include(s => s.Assignments)
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Student)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subject == null) return null;

                var dto = _mapper.Map<SubjectDto>(subject);
                dto.TotalAssignments = subject.Assignments?.Count ?? 0;
                dto.ActiveStudents = subject.ProgressRecords?
                    .Where(p => p.Student.IsActive)
                    .Select(p => p.StudentId)
                    .Distinct()
                    .Count() ?? 0;
                dto.AverageProgress = subject.ProgressRecords?.Any() == true
                    ? subject.ProgressRecords.Average(p => p.CompletionPercentage)
                    : 0;

                await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject with ID {SubjectId}", id);
                throw;
            }
        }

        public async Task<SubjectDetailsDto?> GetSubjectDetailsAsync(int id)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Assignments.Where(a => a.IsActive))
                    .Include(s => s.ProgressRecords.OrderByDescending(p => p.AssessmentDate).Take(10))
                        .ThenInclude(p => p.Student)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subject == null) return null;

                var detailsDto = _mapper.Map<SubjectDetailsDto>(subject);
                detailsDto.Assignments = _mapper.Map<List<AssignmentDto>>(subject.Assignments);
                detailsDto.RecentProgress = _mapper.Map<List<StudentProgressDto>>(subject.ProgressRecords);
                detailsDto.Statistics = await GetSubjectStatisticsAsync(id);

                return detailsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject details for ID {SubjectId}", id);
                throw;
            }
        }

        public async Task<SubjectDto> CreateSubjectAsync(SubjectCreateDto createDto)
        {
            try
            {
                // Check for duplicate code within the same grade
                var existingSubject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.Code == createDto.Code && s.Grade == createDto.Grade);

                if (existingSubject != null)
                {
                    throw new InvalidOperationException($"Subject with code '{createDto.Code}' already exists for grade {createDto.Grade}");
                }

                var subject = _mapper.Map<Subject>(createDto);
                subject.CreatedAt = DateTime.UtcNow;
                subject.UpdatedAt = DateTime.UtcNow;
                subject.IsActive = true;

                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new subject: {SubjectName} ({SubjectCode}) for grade {Grade}", 
                    subject.Name, subject.Code, subject.Grade);

                // Clear related cache
                await _cacheService.RemovePatternAsync("subjects_*");
                await _cacheService.RemovePatternAsync($"subject_grade_{subject.Grade}");

                return _mapper.Map<SubjectDto>(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject: {SubjectName}", createDto.Name);
                throw;
            }
        }

        public async Task<SubjectDto?> UpdateSubjectAsync(int id, SubjectUpdateDto updateDto)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null) return null;

                // Check for duplicate code if code is being updated
                if (!string.IsNullOrWhiteSpace(updateDto.Code) && updateDto.Code != subject.Code)
                {
                    var grade = updateDto.Grade ?? subject.Grade;
                    var existingSubject = await _context.Subjects
                        .FirstOrDefaultAsync(s => s.Code == updateDto.Code && s.Grade == grade && s.Id != id);

                    if (existingSubject != null)
                    {
                        throw new InvalidOperationException($"Subject with code '{updateDto.Code}' already exists for grade {grade}");
                    }
                }

                // Update properties
                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                    subject.Name = updateDto.Name;
                if (!string.IsNullOrWhiteSpace(updateDto.Code))
                    subject.Code = updateDto.Code;
                if (updateDto.Description != null)
                    subject.Description = updateDto.Description;
                if (updateDto.Grade.HasValue)
                    subject.Grade = updateDto.Grade.Value;
                if (updateDto.IsActive.HasValue)
                    subject.IsActive = updateDto.IsActive.Value;

                subject.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated subject: {SubjectId} - {SubjectName}", id, subject.Name);

                // Clear cache
                await _cacheService.RemoveAsync($"subject_{id}");
                await _cacheService.RemovePatternAsync("subjects_*");

                return _mapper.Map<SubjectDto>(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject with ID {SubjectId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSubjectAsync(int id)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Assignments)
                    .Include(s => s.ProgressRecords)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subject == null) return false;

                // Check if subject has associated data
                if (subject.Assignments?.Any() == true || subject.ProgressRecords?.Any() == true)
                {
                    // Soft delete - just mark as inactive
                    subject.IsActive = false;
                    subject.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Soft deleted subject: {SubjectId} - {SubjectName} (has associated data)", id, subject.Name);
                }
                else
                {
                    // Hard delete if no associated data
                    _context.Subjects.Remove(subject);
                    _logger.LogInformation("Hard deleted subject: {SubjectId} - {SubjectName}", id, subject.Name);
                }

                await _context.SaveChangesAsync();

                // Clear cache
                await _cacheService.RemoveAsync($"subject_{id}");
                await _cacheService.RemovePatternAsync("subjects_*");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject with ID {SubjectId}", id);
                throw;
            }
        }

        public async Task<List<SubjectDto>> GetSubjectsByGradeAsync(int grade)
        {
            try
            {
                var cacheKey = $"subjects_grade_{grade}";
                var cached = await _cacheService.GetAsync<List<SubjectDto>>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var subjects = await _context.Subjects
                    .Where(s => s.Grade == grade && s.IsActive)
                    .Include(s => s.Assignments)
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Student)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                var subjectDtos = subjects.Select(s =>
                {
                    var dto = _mapper.Map<SubjectDto>(s);
                    dto.TotalAssignments = s.Assignments?.Count ?? 0;
                    dto.ActiveStudents = s.ProgressRecords?
                        .Where(p => p.Student.IsActive)
                        .Select(p => p.StudentId)
                        .Distinct()
                        .Count() ?? 0;
                    dto.AverageProgress = s.ProgressRecords?.Any() == true
                        ? s.ProgressRecords.Average(p => p.CompletionPercentage)
                        : 0;
                    return dto;
                }).ToList();

                await _cacheService.SetAsync(cacheKey, subjectDtos, TimeSpan.FromHours(1));
                return subjectDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subjects for grade {Grade}", grade);
                throw;
            }
        }

        public async Task<SubjectStatisticsDto> GetSubjectStatisticsAsync(int id)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Assignments)
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Student)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subject == null)
                {
                    throw new InvalidOperationException($"Subject with ID {id} not found");
                }

                var progressRecords = subject.ProgressRecords?.ToList() ?? new List<StudentProgress>();
                var totalStudents = progressRecords.Select(p => p.StudentId).Distinct().Count();
                var activeStudents = progressRecords.Where(p => p.Student.IsActive).Select(p => p.StudentId).Distinct().Count();

                var statistics = new SubjectStatisticsDto
                {
                    TotalStudentsEnrolled = totalStudents,
                    ActiveStudents = activeStudents,
                    AverageCompletionRate = progressRecords.Any() ? progressRecords.Average(p => p.CompletionPercentage) : 0,
                    AveragePerformanceScore = progressRecords.Any() ? progressRecords.Average(p => p.PerformanceScore) : 0,
                    TotalAssignments = subject.Assignments?.Count ?? 0,
                    CompletedAssignments = subject.Assignments?.Count(a => a.DueDate < DateTime.UtcNow) ?? 0,
                    LastActivityDate = progressRecords.Any() ? progressRecords.Max(p => p.LastActivityDate) : null
                };

                // Calculate performance level distribution
                var performanceLevels = new Dictionary<string, int>
                {
                    ["Struggling"] = 0,
                    ["OnTrack"] = 0,
                    ["Advanced"] = 0
                };

                var studentProgressGroups = progressRecords
                    .GroupBy(p => p.StudentId)
                    .ToList();

                foreach (var studentGroup in studentProgressGroups)
                {
                    var avgPerformance = studentGroup.Average(p => p.PerformanceScore);
                    var avgCompletion = studentGroup.Average(p => p.CompletionPercentage);

                    if (avgPerformance >= 85 && avgCompletion >= 90)
                        performanceLevels["Advanced"]++;
                    else if (avgPerformance >= 70 && avgCompletion >= 70)
                        performanceLevels["OnTrack"]++;
                    else
                        performanceLevels["Struggling"]++;
                }

                statistics.PerformanceLevels = performanceLevels.Select(kvp => new PerformanceLevelDistributionDto
                {
                    Level = kvp.Key,
                    StudentCount = kvp.Value,
                    Percentage = totalStudents > 0 ? (decimal)kvp.Value / totalStudents * 100 : 0
                }).ToList();

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating statistics for subject ID {SubjectId}", id);
                throw;
            }
        }
    }
}