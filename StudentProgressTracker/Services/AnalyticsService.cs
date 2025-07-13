using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Services.Interfaces;

namespace StudentProgressTracker.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<AnalyticsService> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AnalyticsDto.ClassSummaryDto> GetClassSummaryAsync(int? grade = null, string? subject = null)
        {
            try
            {
                var cacheKey = $"class_summary_{grade}_{subject}";
                var cached = await _cacheService.GetAsync<AnalyticsDto.ClassSummaryDto>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var studentsQuery = _context.Students
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Subject)
                    .AsQueryable();

                if (grade.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.Grade == grade.Value);
                }

                var students = await studentsQuery.ToListAsync();

                var progressQuery = _context.StudentProgress
                    .Include(p => p.Student)
                    .Include(p => p.Subject)
                    .AsQueryable();

                if (grade.HasValue)
                {
                    progressQuery = progressQuery.Where(p => p.Student.Grade == grade.Value);
                }

                if (!string.IsNullOrWhiteSpace(subject))
                {
                    progressQuery = progressQuery.Where(p => 
                        p.Subject.Name.Contains(subject) || p.Subject.Code.Contains(subject));
                }

                var progressRecords = await progressQuery.ToListAsync();

                var totalStudents = students.Count;
                var activeStudents = students.Count(s => s.IsActive);

                var averagePerformance = progressRecords.Any() 
                    ? progressRecords.Average(p => p.PerformanceScore) 
                    : 0;

                var averageCompletion = progressRecords.Any() 
                    ? progressRecords.Average(p => p.CompletionPercentage) 
                    : 0;

                // Calculate progress level distribution
                var studentsStruggling = 0;
                var studentsOnTrack = 0;
                var studentsAdvanced = 0;

                foreach (var student in students)
                {
                    var studentProgress = progressRecords.Where(p => p.StudentId == student.Id).ToList();
                    if (!studentProgress.Any()) continue;

                    var avgPerformance = studentProgress.Average(p => p.PerformanceScore);
                    var avgCompletion = studentProgress.Average(p => p.CompletionPercentage);

                    if (avgPerformance >= 85 && avgCompletion >= 90)
                        studentsAdvanced++;
                    else if (avgPerformance >= 70 && avgCompletion >= 70)
                        studentsOnTrack++;
                    else
                        studentsStruggling++;
                }

                // Calculate subject performance
                var subjectPerformance = progressRecords
                    .GroupBy(p => new { p.Subject.Name, p.SubjectId })
                    .Select(g => new AnalyticsDto.SubjectPerformanceDto
                    {
                        SubjectName = g.Key.Name,
                        AverageScore = g.Average(p => p.PerformanceScore),
                        CompletionRate = g.Average(p => p.CompletionPercentage),
                        StudentCount = g.Select(p => p.StudentId).Distinct().Count()
                    })
                    .OrderByDescending(s => s.AverageScore)
                    .ToList();

                var result = new AnalyticsDto.ClassSummaryDto
                {
                    TotalStudents = totalStudents,
                    ActiveStudents = activeStudents,
                    AveragePerformance = averagePerformance,
                    AverageCompletion = averageCompletion,
                    StudentsStruggling = studentsStruggling,
                    StudentsOnTrack = studentsOnTrack,
                    StudentsAdvanced = studentsAdvanced,
                    SubjectPerformance = subjectPerformance
                };

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating class summary for grade {Grade}, subject {Subject}", grade, subject);
                throw;
            }
        }

        public async Task<AnalyticsDto.ProgressTrendsDto> GetProgressTrendsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string period = "Monthly")
        {
            try
            {
                var cacheKey = $"progress_trends_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_{period}";
                var cached = await _cacheService.GetAsync<AnalyticsDto.ProgressTrendsDto>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                startDate ??= DateTime.UtcNow.AddMonths(-6);
                endDate ??= DateTime.UtcNow;

                var progressRecords = await _context.StudentProgress
                    .Include(p => p.Student)
                    .Where(p => p.AssessmentDate >= startDate && p.AssessmentDate <= endDate)
                    .OrderBy(p => p.AssessmentDate)
                    .ToListAsync();

                var dataPoints = new List<AnalyticsDto.ProgressDataPointDto>();

                if (period.ToLower() == "weekly")
                {
                    var weeklyGroups = progressRecords
                        .GroupBy(p => new 
                        { 
                            Year = p.AssessmentDate.Year,
                            Week = GetWeekOfYear(p.AssessmentDate)
                        })
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Week);

                    foreach (var group in weeklyGroups)
                    {
                        var weekStart = GetFirstDateOfWeek(group.Key.Year, group.Key.Week);
                        dataPoints.Add(new AnalyticsDto.ProgressDataPointDto
                        {
                            Date = weekStart,
                            AveragePerformance = group.Average(p => p.PerformanceScore),
                            AverageCompletion = group.Average(p => p.CompletionPercentage),
                            ActiveStudents = group.Select(p => p.StudentId).Distinct().Count()
                        });
                    }
                }
                else if (period.ToLower() == "quarterly")
                {
                    var quarterlyGroups = progressRecords
                        .GroupBy(p => new 
                        { 
                            Year = p.AssessmentDate.Year,
                            Quarter = (p.AssessmentDate.Month - 1) / 3 + 1
                        })
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Quarter);

                    foreach (var group in quarterlyGroups)
                    {
                        var quarterStart = new DateTime(group.Key.Year, (group.Key.Quarter - 1) * 3 + 1, 1);
                        dataPoints.Add(new AnalyticsDto.ProgressDataPointDto
                        {
                            Date = quarterStart,
                            AveragePerformance = group.Average(p => p.PerformanceScore),
                            AverageCompletion = group.Average(p => p.CompletionPercentage),
                            ActiveStudents = group.Select(p => p.StudentId).Distinct().Count()
                        });
                    }
                }
                else // Monthly (default)
                {
                    var monthlyGroups = progressRecords
                        .GroupBy(p => new { p.AssessmentDate.Year, p.AssessmentDate.Month })
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Month);

                    foreach (var group in monthlyGroups)
                    {
                        var monthStart = new DateTime(group.Key.Year, group.Key.Month, 1);
                        dataPoints.Add(new AnalyticsDto.ProgressDataPointDto
                        {
                            Date = monthStart,
                            AveragePerformance = group.Average(p => p.PerformanceScore),
                            AverageCompletion = group.Average(p => p.CompletionPercentage),
                            ActiveStudents = group.Select(p => p.StudentId).Distinct().Count()
                        });
                    }
                }

                var result = new AnalyticsDto.ProgressTrendsDto
                {
                    DataPoints = dataPoints,
                    Period = period
                };

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating progress trends for period {Period}", period);
                throw;
            }
        }

        private int GetWeekOfYear(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = (int)jan1.DayOfWeek;
            var firstWeekDay = jan1.AddDays(-daysOffset);
            var weekNumber = ((date - firstWeekDay).Days / 7) + 1;
            return weekNumber;
        }

        private DateTime GetFirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = (int)jan1.DayOfWeek;
            var firstWeekDay = jan1.AddDays(-daysOffset);
            return firstWeekDay.AddDays((weekOfYear - 1) * 7);
        }
    }
}
