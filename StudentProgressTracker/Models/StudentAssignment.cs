using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProgressTracker.Models
{
    public class StudentAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        public DateTime? SubmissionDate { get; set; }

        [Range(0, 100)]
        public decimal? Score { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // "NotStarted", "InProgress", "Submitted", "Graded"

        [StringLength(1000)]
        public string? TeacherFeedback { get; set; }

        public int TimeSpentMinutes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; } = null!;
    }
}
