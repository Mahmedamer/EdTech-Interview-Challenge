using StudentProgressTracker.DTOs;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services.Interfaces
{
    public interface IStudentService
    {
        Task<PaginatedResponseDto<StudentDto>> GetStudentsAsync(
            int page = 1, 
            int pageSize = 10, 
            int? grade = null, 
            string? subject = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true);

        Task<StudentDto?> GetStudentByIdAsync(int id);
        Task<StudentDto> CreateStudentAsync(StudentCreateDto createDto);
        Task<StudentDto?> UpdateStudentAsync(int id, StudentUpdateDto updateDto);
        Task<bool> DeleteStudentAsync(int id);
        Task<List<StudentProgressDto>> GetStudentProgressAsync(int studentId);
        Task<StudentProgressDto> CreateStudentProgressAsync(StudentProgressCreateDto progressDto);
        Task<List<StudentDto>> GetStudentsByTeacherAsync(int teacherId);
    }

    public interface ISubjectService
    {
        Task<PaginatedResponseDto<SubjectDto>> GetSubjectsAsync(
            int page = 1,
            int pageSize = 10,
            int? grade = null,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            bool? isActive = null);

        Task<SubjectDto?> GetSubjectByIdAsync(int id);
        Task<SubjectDetailsDto?> GetSubjectDetailsAsync(int id);
        Task<SubjectDto> CreateSubjectAsync(SubjectCreateDto createDto);
        Task<SubjectDto?> UpdateSubjectAsync(int id, SubjectUpdateDto updateDto);
        Task<bool> DeleteSubjectAsync(int id);
        Task<List<SubjectDto>> GetSubjectsByGradeAsync(int grade);
        Task<SubjectStatisticsDto> GetSubjectStatisticsAsync(int id);
    }

    public interface ITeacherService
    {
        Task<PaginatedResponseDto<TeacherDto>> GetTeachersAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            bool? isActive = null);

        Task<TeacherDto?> GetTeacherByIdAsync(int id);
        Task<TeacherDetailsDto?> GetTeacherDetailsAsync(int id);
        Task<TeacherDto> CreateTeacherAsync(TeacherCreateDto createDto);
        Task<TeacherDto?> UpdateTeacherAsync(int id, TeacherUpdateDto updateDto);
        Task<bool> DeleteTeacherAsync(int id);
        Task<List<TeacherDto>> GetActiveTeachersAsync();
        Task<TeacherStatisticsDto> GetTeacherStatisticsAsync(int id);
        Task<List<StudentDto>> GetTeacherStudentsAsync(int teacherId);
        Task<TeacherStudentAssignmentDto> AssignStudentToTeacherAsync(TeacherStudentAssignmentCreateDto assignmentDto);
        Task<bool> UnassignStudentFromTeacherAsync(int teacherId, int studentId);
        Task<List<TeacherStudentAssignmentDto>> BulkAssignStudentsToTeacherAsync(BulkTeacherStudentAssignmentDto bulkAssignmentDto);
        Task<bool> UpdateTeacherStudentAssignmentAsync(int assignmentId, TeacherStudentAssignmentUpdateDto updateDto);
    }

    public interface IAuthService
    {
        Task<AuthDto.LoginResponseDto?> LoginAsync(AuthDto.LoginRequestDto loginRequest);
        Task<AuthDto.UserProfileDto?> GetUserProfileAsync(int userId);
        string GenerateJwtToken(User user);
        int? ValidateJwtToken(string token);
    }

    public interface IAnalyticsService
    {
        Task<AnalyticsDto.ClassSummaryDto> GetClassSummaryAsync(int? grade = null, string? subject = null);
        Task<AnalyticsDto.ProgressTrendsDto> GetProgressTrendsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null,
            string period = "Monthly");
    }

    public interface IReportService
    {
        Task<ReportDto.ExportResultDto> ExportStudentDataAsync(ReportDto.ExportOptionsDto options);
        Task<ReportDto.ReportResultDto?> GenerateStudentProgressReportAsync(ReportDto.StudentReportOptionsDto options);
        Task<ReportDto.ReportResultDto> GenerateClassSummaryReportAsync(ReportDto.ClassReportOptionsDto options);
        Task<byte[]> ExportStudentDataAsync(
            int? grade = null, 
            string? subject = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null);
    }

    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task RemoveAsync(string key);
        Task RemovePatternAsync(string pattern);
    }
}
