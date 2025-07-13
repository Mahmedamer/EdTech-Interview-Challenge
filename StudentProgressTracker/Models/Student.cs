using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProgressTracker.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(1, 12)]
        public int Grade { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public DateTime EnrollmentDate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<StudentProgress> ProgressRecords { get; set; } = new List<StudentProgress>();
        public virtual ICollection<StudentAssignment> StudentAssignments { get; set; } = new List<StudentAssignment>();
        public virtual ICollection<TeacherStudent> TeacherStudents { get; set; } = new List<TeacherStudent>();

        // Computed properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - 
                         (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }
}
