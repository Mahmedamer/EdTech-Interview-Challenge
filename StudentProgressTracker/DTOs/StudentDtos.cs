namespace StudentProgressTracker.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Grade { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime LastActivityDate { get; set; }
        public decimal OverallProgress { get; set; }
        public string ProgressLevel { get; set; } = string.Empty;
    }

    public class StudentCreateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Grade { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }

    public class StudentUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? Grade { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class StudentProgressDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public decimal CompletionPercentage { get; set; }
        public decimal PerformanceScore { get; set; }
        public int TimeSpentMinutes { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string ProgressLevel { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime LastActivityDate { get; set; }
    }

    public class StudentProgressCreateDto
    {
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public decimal CompletionPercentage { get; set; }
        public decimal PerformanceScore { get; set; }
        public int TimeSpentMinutes { get; set; }
        public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
        public string ProgressLevel { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
