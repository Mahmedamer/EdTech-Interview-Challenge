using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Services.Interfaces;
using System.Text;

namespace StudentProgressTracker.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<byte[]> ExportStudentDataAsync(
            int? grade = null,
            string? subject = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var studentsQuery = _context.Students
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Subject)
                    .AsQueryable();

                if (grade.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.Grade == grade.Value);
                }

                var students = await studentsQuery.ToListAsync();

                var csv = new StringBuilder();

                // Header
                csv.AppendLine("StudentId,FirstName,LastName,Email,Grade,DateOfBirth,Age,EnrollmentDate,IsActive," +
                              "SubjectName,SubjectCode,CompletionPercentage,PerformanceScore,TimeSpentMinutes," +
                              "AssessmentDate,ProgressLevel,LastActivityDate,Notes");

                foreach (var student in students)
                {
                    var progressRecords = student.ProgressRecords.AsQueryable();

                    // Apply subject filter
                    if (!string.IsNullOrWhiteSpace(subject))
                    {
                        progressRecords = progressRecords.Where(p => 
                            p.Subject.Name.Contains(subject) || p.Subject.Code.Contains(subject));
                    }

                    // Apply date filter
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        progressRecords = progressRecords.Where(p => 
                            (!startDate.HasValue || p.AssessmentDate >= startDate.Value) &&
                            (!endDate.HasValue || p.AssessmentDate <= endDate.Value));
                    }

                    var filteredProgress = progressRecords.ToList();

                    if (filteredProgress.Any())
                    {
                        foreach (var progress in filteredProgress)
                        {
                            csv.AppendLine($"{student.Id}," +
                                         $"\"{EscapeCsvValue(student.FirstName)}\"," +
                                         $"\"{EscapeCsvValue(student.LastName)}\"," +
                                         $"\"{EscapeCsvValue(student.Email)}\"," +
                                         $"{student.Grade}," +
                                         $"{student.DateOfBirth:yyyy-MM-dd}," +
                                         $"{student.Age}," +
                                         $"{student.EnrollmentDate:yyyy-MM-dd}," +
                                         $"{student.IsActive}," +
                                         $"\"{EscapeCsvValue(progress.Subject.Name)}\"," +
                                         $"\"{EscapeCsvValue(progress.Subject.Code)}\"," +
                                         $"{progress.CompletionPercentage}," +
                                         $"{progress.PerformanceScore}," +
                                         $"{progress.TimeSpentMinutes}," +
                                         $"{progress.AssessmentDate:yyyy-MM-dd HH:mm:ss}," +
                                         $"\"{EscapeCsvValue(progress.ProgressLevel)}\"," +
                                         $"{progress.LastActivityDate:yyyy-MM-dd HH:mm:ss}," +
                                         $"\"{EscapeCsvValue(progress.Notes ?? "")}\"");
                        }
                    }
                    else
                    {
                        // Include student even if no progress records match the filter
                        csv.AppendLine($"{student.Id}," +
                                     $"\"{EscapeCsvValue(student.FirstName)}\"," +
                                     $"\"{EscapeCsvValue(student.LastName)}\"," +
                                     $"\"{EscapeCsvValue(student.Email)}\"," +
                                     $"{student.Grade}," +
                                     $"{student.DateOfBirth:yyyy-MM-dd}," +
                                     $"{student.Age}," +
                                     $"{student.EnrollmentDate:yyyy-MM-dd}," +
                                     $"{student.IsActive}," +
                                     $"\"\"," + // SubjectName
                                     $"\"\"," + // SubjectCode
                                     $"0," +    // CompletionPercentage
                                     $"0," +    // PerformanceScore
                                     $"0," +    // TimeSpentMinutes
                                     $"\"\"," + // AssessmentDate
                                     $"\"New\"," + // ProgressLevel
                                     $"\"\"," + // LastActivityDate
                                     $"\"\"");  // Notes
                    }
                }

                _logger.LogInformation("Generated CSV export for {StudentCount} students", students.Count);
                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student data export");
                throw;
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Escape quotes by doubling them
            return value.Replace("\"", "\"\"");
        }

        public async Task<ReportDto.ExportResultDto> ExportStudentDataAsync(ReportDto.ExportOptionsDto options)
        {
            try
            {
                var data = await ExportStudentDataAsync(options.Grade, options.Subject);
                
                return new ReportDto.ExportResultDto
                {
                    Data = data,
                    FileName = $"student-data-export_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.{options.Format}",
                    ContentType = GetContentType(options.Format),
                    RecordCount = await GetStudentCountAsync(options.Grade, options.Subject),
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting student data");
                throw;
            }
        }

        public async Task<ReportDto.ReportResultDto?> GenerateStudentProgressReportAsync(ReportDto.StudentReportOptionsDto options)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Subject)
                    .FirstOrDefaultAsync(s => s.Id == options.StudentId);

                if (student == null)
                    return null;

                // Generate a simple report - in a real app, this would use a proper report generator
                var reportContent = GenerateStudentProgressReportContent(student, options);
                
                return new ReportDto.ReportResultDto
                {
                    Data = Encoding.UTF8.GetBytes(reportContent),
                    FileName = $"progress-report_student-{options.StudentId}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.{options.Format}",
                    ContentType = GetContentType(options.Format),
                    GeneratedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["studentId"] = options.StudentId,
                        ["studentName"] = student.FullName,
                        ["includeHistory"] = options.IncludeHistory,
                        ["includeRecommendations"] = options.IncludeRecommendations
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student progress report for student {StudentId}", options.StudentId);
                throw;
            }
        }

        public async Task<ReportDto.ReportResultDto> GenerateClassSummaryReportAsync(ReportDto.ClassReportOptionsDto options)
        {
            try
            {
                var studentsQuery = _context.Students
                    .Include(s => s.ProgressRecords)
                        .ThenInclude(p => p.Subject)
                    .AsQueryable();

                if (options.Grade.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.Grade == options.Grade.Value);
                }

                var students = await studentsQuery.ToListAsync();
                
                // Generate class summary report content
                var reportContent = GenerateClassSummaryReportContent(students, options);
                
                return new ReportDto.ReportResultDto
                {
                    Data = Encoding.UTF8.GetBytes(reportContent),
                    FileName = $"class-summary_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.{options.Format}",
                    ContentType = GetContentType(options.Format),
                    GeneratedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["grade"] = options.Grade,
                        ["subject"] = options.Subject,
                        ["includeAnalytics"] = options.IncludeAnalytics,
                        ["studentCount"] = students.Count
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating class summary report");
                throw;
            }
        }

        private string GetContentType(string format)
        {
            return format.ToLower() switch
            {
                "csv" => "text/csv",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        private async Task<int> GetStudentCountAsync(int? grade, string? subject)
        {
            var query = _context.Students.AsQueryable();
            
            if (grade.HasValue)
                query = query.Where(s => s.Grade == grade.Value);
                
            return await query.CountAsync();
        }

        private string GenerateStudentProgressReportContent(Models.Student student, ReportDto.StudentReportOptionsDto options)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"STUDENT PROGRESS REPORT");
            sb.AppendLine($"======================");
            sb.AppendLine($"Student: {student.FullName}");
            sb.AppendLine($"Grade: {student.Grade}");
            sb.AppendLine($"Email: {student.Email}");
            sb.AppendLine($"Age: {student.Age}");
            sb.AppendLine($"Enrollment Date: {student.EnrollmentDate:yyyy-MM-dd}");
            sb.AppendLine();
            
            if (options.IncludeHistory && student.ProgressRecords.Any())
            {
                sb.AppendLine("PROGRESS HISTORY:");
                sb.AppendLine("-----------------");
                foreach (var progress in student.ProgressRecords.OrderByDescending(p => p.AssessmentDate))
                {
                    sb.AppendLine($"Subject: {progress.Subject.Name}");
                    sb.AppendLine($"  Completion: {progress.CompletionPercentage:F1}%");
                    sb.AppendLine($"  Performance: {progress.PerformanceScore:F1}");
                    sb.AppendLine($"  Date: {progress.AssessmentDate:yyyy-MM-dd}");
                    sb.AppendLine();
                }
            }
            
            if (options.IncludeRecommendations)
            {
                sb.AppendLine("RECOMMENDATIONS:");
                sb.AppendLine("----------------");
                var avgCompletion = student.ProgressRecords.Any() ? student.ProgressRecords.Average(p => p.CompletionPercentage) : 0;
                var avgPerformance = student.ProgressRecords.Any() ? student.ProgressRecords.Average(p => p.PerformanceScore) : 0;
                
                if (avgCompletion < 70)
                    sb.AppendLine("- Consider additional support for assignment completion");
                if (avgPerformance < 70)
                    sb.AppendLine("- Review understanding of key concepts");
                if (avgPerformance > 90)
                    sb.AppendLine("- Consider advanced or enrichment activities");
                
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private string GenerateClassSummaryReportContent(List<Models.Student> students, ReportDto.ClassReportOptionsDto options)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CLASS SUMMARY REPORT");
            sb.AppendLine($"===================");
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"Total Students: {students.Count}");
            
            if (options.Grade.HasValue)
                sb.AppendLine($"Grade Filter: {options.Grade.Value}");
            if (!string.IsNullOrEmpty(options.Subject))
                sb.AppendLine($"Subject Filter: {options.Subject}");
                
            sb.AppendLine();
            
            if (options.IncludeAnalytics && students.Any())
            {
                sb.AppendLine("CLASS ANALYTICS:");
                sb.AppendLine("----------------");
                
                var allProgress = students.SelectMany(s => s.ProgressRecords).ToList();
                if (allProgress.Any())
                {
                    var avgCompletion = allProgress.Average(p => p.CompletionPercentage);
                    var avgPerformance = allProgress.Average(p => p.PerformanceScore);
                    
                    sb.AppendLine($"Average Completion: {avgCompletion:F1}%");
                    sb.AppendLine($"Average Performance: {avgPerformance:F1}");
                    
                    var struggling = students.Count(s => s.ProgressRecords.Any(p => p.PerformanceScore < 70));
                    var onTrack = students.Count(s => s.ProgressRecords.Any(p => p.PerformanceScore >= 70 && p.PerformanceScore < 85));
                    var advanced = students.Count(s => s.ProgressRecords.Any(p => p.PerformanceScore >= 85));
                    
                    sb.AppendLine($"Students Struggling: {struggling}");
                    sb.AppendLine($"Students On Track: {onTrack}");
                    sb.AppendLine($"Advanced Students: {advanced}");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("STUDENT LIST:");
            sb.AppendLine("-------------");
            foreach (var student in students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName))
            {
                var avgPerformance = student.ProgressRecords.Any() ? student.ProgressRecords.Average(p => p.PerformanceScore) : 0;
                sb.AppendLine($"{student.FullName} (Grade {student.Grade}) - Avg Performance: {avgPerformance:F1}");
            }
            
            return sb.ToString();
        }
    }
}
