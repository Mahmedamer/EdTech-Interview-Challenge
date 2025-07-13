namespace StudentProgressTracker.DTOs
{
    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int MaxPoints { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AssignmentCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SubjectId { get; set; }
        public DateTime DueDate { get; set; }
        public int MaxPoints { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
    }

    public class AssignmentUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? MaxPoints { get; set; }
        public string? AssignmentType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class StudentAssignmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public DateTime? SubmissionDate { get; set; }
        public decimal? Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TeacherFeedback { get; set; }
        public int TimeSpentMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class StudentAssignmentCreateDto
    {
        public int StudentId { get; set; }
        public int AssignmentId { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public decimal? Score { get; set; }
        public string Status { get; set; } = "NotStarted";
        public string? TeacherFeedback { get; set; }
        public int TimeSpentMinutes { get; set; }
    }

    public class StudentAssignmentUpdateDto
    {
        public DateTime? SubmissionDate { get; set; }
        public decimal? Score { get; set; }
        public string? Status { get; set; }
        public string? TeacherFeedback { get; set; }
        public int? TimeSpentMinutes { get; set; }
    }
}