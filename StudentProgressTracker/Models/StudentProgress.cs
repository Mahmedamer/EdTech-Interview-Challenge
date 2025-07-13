using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProgressTracker.Models
{
    public class StudentProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal CompletionPercentage { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal PerformanceScore { get; set; }

        [Required]
        public int TimeSpentMinutes { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string ProgressLevel { get; set; } = string.Empty; // "Struggling", "OnTrack", "Advanced"

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime LastActivityDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;
    }
}
