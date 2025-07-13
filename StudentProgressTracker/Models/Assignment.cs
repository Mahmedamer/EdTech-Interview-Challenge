using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProgressTracker.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(0, 100)]
        public int MaxPoints { get; set; }

        [Required]
        [StringLength(50)]
        public string AssignmentType { get; set; } = string.Empty; // "Homework", "Quiz", "Test", "Project"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;

        public virtual ICollection<StudentAssignment> StudentAssignments { get; set; } = new List<StudentAssignment>();
    }
}
