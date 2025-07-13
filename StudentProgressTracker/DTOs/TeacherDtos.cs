using System.ComponentModel.DataAnnotations;

namespace StudentProgressTracker.DTOs
{
    public class TeacherDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public List<string> AssignedGrades { get; set; } = new();
        public TeacherStatisticsDto Statistics { get; set; } = new();
    }

    public class TeacherCreateDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }
    }

    public class TeacherUpdateDto
    {
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        public bool? IsActive { get; set; }
    }

    public class TeacherDetailsDto : TeacherDto
    {
        public List<StudentDto> AssignedStudents { get; set; } = new();
        public List<TeacherStudentAssignmentDto> StudentAssignments { get; set; } = new();
        public TeacherPerformanceDto Performance { get; set; } = new();
    }

    public class TeacherStatisticsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public decimal AverageStudentProgress { get; set; }
        public decimal AverageStudentPerformance { get; set; }
        public int StudentsStruggling { get; set; }
        public int StudentsOnTrack { get; set; }
        public int StudentsAdvanced { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public List<GradeDistributionDto> GradeDistribution { get; set; } = new();
    }

    public class TeacherPerformanceDto
    {
        public decimal OverallEffectiveness { get; set; }
        public decimal StudentEngagement { get; set; }
        public decimal ProgressImprovement { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public List<MonthlyPerformanceDto> MonthlyTrends { get; set; } = new();
    }

    public class MonthlyPerformanceDto
    {
        public DateTime Month { get; set; }
        public decimal AverageProgress { get; set; }
        public decimal AveragePerformance { get; set; }
        public int ActiveStudents { get; set; }
    }

    public class GradeDistributionDto
    {
        public int Grade { get; set; }
        public int StudentCount { get; set; }
        public decimal AverageProgress { get; set; }
    }

    public class TeacherStudentAssignmentDto
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int StudentGrade { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? UnassignedDate { get; set; }
        public bool IsActive { get; set; }
        public decimal StudentProgress { get; set; }
        public string ProgressLevel { get; set; } = string.Empty;
    }

    public class TeacherStudentAssignmentCreateDto
    {
        [Required(ErrorMessage = "Teacher ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be a positive number")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Student ID must be a positive number")]
        public int StudentId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }

    public class TeacherStudentAssignmentUpdateDto
    {
        public DateTime? UnassignedDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class BulkTeacherStudentAssignmentDto
    {
        [Required(ErrorMessage = "Teacher ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be a positive number")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Student IDs are required")]
        [MinLength(1, ErrorMessage = "At least one student ID must be provided")]
        public List<int> StudentIds { get; set; } = new();

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}