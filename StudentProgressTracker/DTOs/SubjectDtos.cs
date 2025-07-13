namespace StudentProgressTracker.DTOs
{
    public class SubjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Grade { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalAssignments { get; set; }
        public int ActiveStudents { get; set; }
        public decimal AverageProgress { get; set; }
    }

    public class SubjectCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Grade { get; set; }
    }

    public class SubjectUpdateDto
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? Grade { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SubjectDetailsDto : SubjectDto
    {
        public List<AssignmentDto> Assignments { get; set; } = new();
        public List<StudentProgressDto> RecentProgress { get; set; } = new();
        public SubjectStatisticsDto Statistics { get; set; } = new();
    }

    public class SubjectStatisticsDto
    {
        public int TotalStudentsEnrolled { get; set; }
        public int ActiveStudents { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public decimal AveragePerformanceScore { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public List<PerformanceLevelDistributionDto> PerformanceLevels { get; set; } = new();
    }

    public class PerformanceLevelDistributionDto
    {
        public string Level { get; set; } = string.Empty; // "Struggling", "OnTrack", "Advanced"
        public int StudentCount { get; set; }
        public decimal Percentage { get; set; }
    }
}