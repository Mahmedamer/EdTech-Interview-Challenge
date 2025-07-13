namespace StudentProgressTracker.DTOs
{
    public class AuthDto
    {
        public class LoginRequestDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponseDto
        {
            public string Token { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public UserProfileDto User { get; set; } = null!;
        }

        public class UserProfileDto
        {
            public int Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public DateTime? LastLoginDate { get; set; }
        }
    }

    public class AnalyticsDto
    {
        public class ClassSummaryDto
        {
            public int TotalStudents { get; set; }
            public int ActiveStudents { get; set; }
            public decimal AveragePerformance { get; set; }
            public decimal AverageCompletion { get; set; }
            public int StudentsStruggling { get; set; }
            public int StudentsOnTrack { get; set; }
            public int StudentsAdvanced { get; set; }
            public List<SubjectPerformanceDto> SubjectPerformance { get; set; } = new();
        }

        public class SubjectPerformanceDto
        {
            public string SubjectName { get; set; } = string.Empty;
            public decimal AverageScore { get; set; }
            public decimal CompletionRate { get; set; }
            public int StudentCount { get; set; }
        }

        public class ProgressTrendsDto
        {
            public List<ProgressDataPointDto> DataPoints { get; set; } = new();
            public string Period { get; set; } = string.Empty; // "Weekly", "Monthly", "Quarterly"
        }

        public class ProgressDataPointDto
        {
            public DateTime Date { get; set; }
            public decimal AveragePerformance { get; set; }
            public decimal AverageCompletion { get; set; }
            public int ActiveStudents { get; set; }
        }
    }

    public class PaginatedResponseDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
