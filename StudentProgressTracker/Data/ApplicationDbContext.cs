using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace StudentProgressTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<StudentProgress> StudentProgress { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<StudentAssignment> StudentAssignments { get; set; }
        public DbSet<TeacherStudent> TeacherStudents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // This will only be used during design-time (migrations)
                optionsBuilder.UseSqlite("Data Source=studentprogress.db");
            }
            
            // Suppress the pending model changes warning for development
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Notes).HasMaxLength(500);
                
                // Configure decimal precision
                entity.Property(e => e.Grade).IsRequired();
            });

            // Subject configuration
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasIndex(e => e.Code); // Index for performance but not unique
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // StudentProgress configuration
            modelBuilder.Entity<StudentProgress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompletionPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.PerformanceScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ProgressLevel).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                // Foreign key relationships
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.ProgressRecords)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.ProgressRecords)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Composite index for performance
                entity.HasIndex(e => new { e.StudentId, e.SubjectId, e.AssessmentDate });
            });

            // Assignment configuration
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.AssignmentType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.Assignments)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // StudentAssignment configuration
            modelBuilder.Entity<StudentAssignment>(entity =>
            {
                entity.Property(e => e.Score).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TeacherFeedback).HasMaxLength(1000);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.StudentAssignments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.StudentAssignments)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint to prevent duplicate assignments
                entity.HasIndex(e => new { e.StudentId, e.AssignmentId }).IsUnique();
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            });

            // TeacherStudent configuration
            modelBuilder.Entity<TeacherStudent>(entity =>
            {
                entity.HasOne(e => e.Teacher)
                    .WithMany(u => u.TeacherStudents)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.TeacherStudents)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint for active teacher-student relationships
                entity.HasIndex(e => new { e.TeacherId, e.StudentId, e.IsActive })
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Subjects
            modelBuilder.Entity<Subject>().HasData(
                new Subject { Id = 1, Name = "Mathematics", Code = "MATH", Grade = 1, Description = "Basic arithmetic and number concepts" },
                new Subject { Id = 2, Name = "Reading", Code = "READ", Grade = 1, Description = "Phonics, sight words, and comprehension" },
                new Subject { Id = 3, Name = "Science", Code = "SCI", Grade = 1, Description = "Nature, simple experiments, and observations" },
                new Subject { Id = 4, Name = "Social Studies", Code = "SS", Grade = 1, Description = "Community, family, and basic geography" },
                new Subject { Id = 5, Name = "Mathematics", Code = "MATH", Grade = 5, Description = "Fractions, decimals, and basic algebra" },
                new Subject { Id = 6, Name = "Reading", Code = "READ", Grade = 5, Description = "Advanced comprehension and writing skills" },
                new Subject { Id = 7, Name = "Science", Code = "SCI", Grade = 5, Description = "Earth science, biology, and scientific method" },
                new Subject { Id = 8, Name = "Social Studies", Code = "SS", Grade = 5, Description = "US history, geography, and civics" },
                new Subject { Id = 9, Name = "Mathematics", Code = "MATH", Grade = 9, Description = "Algebra, geometry, and problem solving" },
                new Subject { Id = 10, Name = "English", Code = "ENG", Grade = 9, Description = "Literature, composition, and critical thinking" },
                new Subject { Id = 11, Name = "Science", Code = "SCI", Grade = 9, Description = "Biology, chemistry, and physics concepts" },
                new Subject { Id = 12, Name = "Social Studies", Code = "SS", Grade = 9, Description = "World history and cultural studies" }
            );

            // Seed Users (Teachers and Admin)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FirstName = "Admin", LastName = "User", Email = "admin@edtech.com", 
                          PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin" },
                new User { Id = 2, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@edtech.com", 
                          PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"), Role = "Teacher" },
                new User { Id = 3, FirstName = "Michael", LastName = "Brown", Email = "michael.brown@edtech.com", 
                          PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"), Role = "Teacher" },
                new User { Id = 4, FirstName = "Jennifer", LastName = "Davis", Email = "jennifer.davis@edtech.com", 
                          PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"), Role = "Teacher" }
            );

            // Seed Students (20+ diverse records across K-12)
            modelBuilder.Entity<Student>().HasData(
                // Grade 1 Students
                new Student { Id = 1, FirstName = "Emma", LastName = "Wilson", Email = "emma.wilson@school.edu", Grade = 1, DateOfBirth = new DateTime(2018, 3, 15), Notes = "Enthusiastic learner, loves reading" },
                new Student { Id = 2, FirstName = "Liam", LastName = "Anderson", Email = "liam.anderson@school.edu", Grade = 1, DateOfBirth = new DateTime(2018, 7, 22), Notes = "Struggles with math concepts, needs extra support" },
                new Student { Id = 3, FirstName = "Sophia", LastName = "Martinez", Email = "sophia.martinez@school.edu", Grade = 1, DateOfBirth = new DateTime(2018, 1, 8), Notes = "Advanced reader, quiet in class" },
                
                // Grade 3 Students
                new Student { Id = 4, FirstName = "Noah", LastName = "Thompson", Email = "noah.thompson@school.edu", Grade = 3, DateOfBirth = new DateTime(2016, 9, 12), Notes = "Good at problem solving, active participant" },
                new Student { Id = 5, FirstName = "Olivia", LastName = "Garcia", Email = "olivia.garcia@school.edu", Grade = 3, DateOfBirth = new DateTime(2016, 5, 30), Notes = "Excellent in science, curious about nature" },
                new Student { Id = 6, FirstName = "Ethan", LastName = "Rodriguez", Email = "ethan.rodriguez@school.edu", Grade = 3, DateOfBirth = new DateTime(2016, 11, 3), Notes = "Sporadic attendance, needs encouragement" },
                
                // Grade 5 Students
                new Student { Id = 7, FirstName = "Ava", LastName = "Lee", Email = "ava.lee@school.edu", Grade = 5, DateOfBirth = new DateTime(2014, 2, 18), Notes = "Math whiz, helps other students" },
                new Student { Id = 8, FirstName = "Mason", LastName = "White", Email = "mason.white@school.edu", Grade = 5, DateOfBirth = new DateTime(2014, 8, 7), Notes = "Struggles with reading comprehension" },
                new Student { Id = 9, FirstName = "Isabella", LastName = "Harris", Email = "isabella.harris@school.edu", Grade = 5, DateOfBirth = new DateTime(2014, 4, 25), Notes = "Well-rounded student, consistent performer" },
                new Student { Id = 10, FirstName = "William", LastName = "Clark", Email = "william.clark@school.edu", Grade = 5, DateOfBirth = new DateTime(2014, 10, 14), Notes = "Creative writer, loves social studies" },
                
                // Grade 7 Students
                new Student { Id = 11, FirstName = "Mia", LastName = "Lewis", Email = "mia.lewis@school.edu", Grade = 7, DateOfBirth = new DateTime(2012, 6, 9), Notes = "Strong in all subjects, leadership qualities" },
                new Student { Id = 12, FirstName = "James", LastName = "Walker", Email = "james.walker@school.edu", Grade = 7, DateOfBirth = new DateTime(2012, 12, 1), Notes = "Inactive lately, needs motivation" },
                new Student { Id = 13, FirstName = "Charlotte", LastName = "Hall", Email = "charlotte.hall@school.edu", Grade = 7, DateOfBirth = new DateTime(2012, 3, 27), Notes = "Excels in science, future engineer" },
                
                // Grade 9 Students
                new Student { Id = 14, FirstName = "Benjamin", LastName = "Young", Email = "benjamin.young@school.edu", Grade = 9, DateOfBirth = new DateTime(2010, 9, 16), Notes = "Advanced in mathematics, peer tutor" },
                new Student { Id = 15, FirstName = "Amelia", LastName = "King", Email = "amelia.king@school.edu", Grade = 9, DateOfBirth = new DateTime(2010, 1, 11), Notes = "Gifted writer, participates in debate team" },
                new Student { Id = 16, FirstName = "Lucas", LastName = "Wright", Email = "lucas.wright@school.edu", Grade = 9, DateOfBirth = new DateTime(2010, 5, 23), Notes = "Struggling student, requires additional support" },
                new Student { Id = 17, FirstName = "Harper", LastName = "Lopez", Email = "harper.lopez@school.edu", Grade = 9, DateOfBirth = new DateTime(2010, 7, 4), Notes = "Inconsistent attendance, family issues" },
                
                // Grade 11 Students
                new Student { Id = 18, FirstName = "Alexander", LastName = "Hill", Email = "alexander.hill@school.edu", Grade = 11, DateOfBirth = new DateTime(2008, 11, 29), Notes = "AP student, college prep track" },
                new Student { Id = 19, FirstName = "Evelyn", LastName = "Green", Email = "evelyn.green@school.edu", Grade = 11, DateOfBirth = new DateTime(2008, 4, 17), Notes = "Strong in humanities, weak in STEM" },
                new Student { Id = 20, FirstName = "Henry", LastName = "Adams", Email = "henry.adams@school.edu", Grade = 11, DateOfBirth = new DateTime(2008, 8, 6), Notes = "Athlete, needs academic balance" },
                
                // Grade 12 Students
                new Student { Id = 21, FirstName = "Abigail", LastName = "Baker", Email = "abigail.baker@school.edu", Grade = 12, DateOfBirth = new DateTime(2007, 2, 13), Notes = "Valedictorian candidate, exceptional student" },
                new Student { Id = 22, FirstName = "Michael", LastName = "Nelson", Email = "michael.nelson@school.edu", Grade = 12, DateOfBirth = new DateTime(2007, 10, 20), Notes = "Late bloomer, improving rapidly" },
                new Student { Id = 23, FirstName = "Emily", LastName = "Carter", Email = "emily.carter@school.edu", Grade = 12, DateOfBirth = new DateTime(2007, 6, 8), Notes = "Art-focused student, creative thinker" }
            );

            // Seed Assignments
            modelBuilder.Entity<Assignment>().HasData(
                // Grade 1 Assignments
                new Assignment { Id = 1, Title = "Number Recognition 1-20", Description = "Identify and write numbers 1-20", SubjectId = 1, AssignmentType = "Practice", DueDate = new DateTime(2025, 1, 15), MaxPoints = 20 },
                new Assignment { Id = 2, Title = "Sight Words Week 1", Description = "Learn first 10 sight words", SubjectId = 2, AssignmentType = "Quiz", DueDate = new DateTime(2025, 1, 18), MaxPoints = 10 },
                new Assignment { Id = 3, Title = "Weather Observation", Description = "Daily weather tracking for one week", SubjectId = 3, AssignmentType = "Project", DueDate = new DateTime(2025, 1, 25), MaxPoints = 25 },
                
                // Grade 5 Assignments
                new Assignment { Id = 4, Title = "Fraction Addition", Description = "Adding fractions with like denominators", SubjectId = 5, AssignmentType = "Homework", DueDate = new DateTime(2025, 2, 1), MaxPoints = 30 },
                new Assignment { Id = 5, Title = "Book Report: Charlotte's Web", Description = "Complete book report with analysis", SubjectId = 6, AssignmentType = "Project", DueDate = new DateTime(2025, 2, 15), MaxPoints = 50 },
                new Assignment { Id = 6, Title = "Solar System Model", Description = "Create 3D model of solar system", SubjectId = 7, AssignmentType = "Project", DueDate = new DateTime(2025, 2, 20), MaxPoints = 40 },
                
                // Grade 9 Assignments
                new Assignment { Id = 7, Title = "Quadratic Equations Test", Description = "Solving quadratic equations unit test", SubjectId = 9, AssignmentType = "Test", DueDate = new DateTime(2025, 3, 5), MaxPoints = 100 },
                new Assignment { Id = 8, Title = "Romeo and Juliet Essay", Description = "5-paragraph essay on themes in Romeo and Juliet", SubjectId = 10, AssignmentType = "Essay", DueDate = new DateTime(2025, 3, 10), MaxPoints = 100 },
                new Assignment { Id = 9, Title = "Cell Structure Lab", Description = "Microscope lab on cell structures", SubjectId = 11, AssignmentType = "Lab", DueDate = new DateTime(2025, 3, 15), MaxPoints = 75 }
            );

            // Seed Teacher-Student Relationships
            modelBuilder.Entity<TeacherStudent>().HasData(
                // Sarah Johnson (Grade 1 teacher) - Students 1-3
                new TeacherStudent { Id = 1, TeacherId = 2, StudentId = 1, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 2, TeacherId = 2, StudentId = 2, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 3, TeacherId = 2, StudentId = 3, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                
                // Michael Brown (Grade 5 teacher) - Students 7-10
                new TeacherStudent { Id = 4, TeacherId = 3, StudentId = 7, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 5, TeacherId = 3, StudentId = 8, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 6, TeacherId = 3, StudentId = 9, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 7, TeacherId = 3, StudentId = 10, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                
                // Jennifer Davis (High school teacher) - Students 14-17, 18-23
                new TeacherStudent { Id = 8, TeacherId = 4, StudentId = 14, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 9, TeacherId = 4, StudentId = 15, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 10, TeacherId = 4, StudentId = 16, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 11, TeacherId = 4, StudentId = 18, AssignedDate = new DateTime(2024, 9, 1), IsActive = true },
                new TeacherStudent { Id = 12, TeacherId = 4, StudentId = 21, AssignedDate = new DateTime(2024, 9, 1), IsActive = true }
            );

            // Seed Student Progress (spanning several months with various levels)
            modelBuilder.Entity<StudentProgress>().HasData(
                // Emma Wilson (Grade 1) - Advanced student
                new StudentProgress { Id = 1, StudentId = 1, SubjectId = 1, AssessmentDate = new DateTime(2024, 10, 15), CompletionPercentage = 95.0m, PerformanceScore = 92.0m, ProgressLevel = "Advanced", Notes = "Excellent number recognition skills" },
                new StudentProgress { Id = 2, StudentId = 1, SubjectId = 2, AssessmentDate = new DateTime(2024, 11, 15), CompletionPercentage = 88.0m, PerformanceScore = 90.0m, ProgressLevel = "Advanced", Notes = "Reading above grade level" },
                
                // Liam Anderson (Grade 1) - Struggling student
                new StudentProgress { Id = 3, StudentId = 2, SubjectId = 1, AssessmentDate = new DateTime(2024, 10, 15), CompletionPercentage = 45.0m, PerformanceScore = 52.0m, ProgressLevel = "Below Basic", Notes = "Needs additional math support" },
                new StudentProgress { Id = 4, StudentId = 2, SubjectId = 2, AssessmentDate = new DateTime(2024, 11, 15), CompletionPercentage = 60.0m, PerformanceScore = 65.0m, ProgressLevel = "Basic", Notes = "Showing improvement with phonics" },
                
                // Ava Lee (Grade 5) - Math whiz
                new StudentProgress { Id = 5, StudentId = 7, SubjectId = 5, AssessmentDate = new DateTime(2024, 12, 1), CompletionPercentage = 98.0m, PerformanceScore = 97.0m, ProgressLevel = "Advanced", Notes = "Exceptional mathematical reasoning" },
                new StudentProgress { Id = 6, StudentId = 7, SubjectId = 7, AssessmentDate = new DateTime(2024, 12, 10), CompletionPercentage = 85.0m, PerformanceScore = 88.0m, ProgressLevel = "Proficient", Notes = "Strong science understanding" },
                
                // Mason White (Grade 5) - Reading struggles
                new StudentProgress { Id = 7, StudentId = 8, SubjectId = 6, AssessmentDate = new DateTime(2024, 12, 1), CompletionPercentage = 55.0m, PerformanceScore = 58.0m, ProgressLevel = "Basic", Notes = "Reading comprehension needs work" },
                new StudentProgress { Id = 8, StudentId = 8, SubjectId = 5, AssessmentDate = new DateTime(2024, 12, 5), CompletionPercentage = 72.0m, PerformanceScore = 75.0m, ProgressLevel = "Proficient", Notes = "Better with math calculations" },
                
                // Benjamin Young (Grade 9) - Advanced student
                new StudentProgress { Id = 9, StudentId = 14, SubjectId = 9, AssessmentDate = new DateTime(2025, 1, 10), CompletionPercentage = 96.0m, PerformanceScore = 94.0m, ProgressLevel = "Advanced", Notes = "Consistently high performance" },
                new StudentProgress { Id = 10, StudentId = 14, SubjectId = 11, AssessmentDate = new DateTime(2025, 1, 15), CompletionPercentage = 89.0m, PerformanceScore = 91.0m, ProgressLevel = "Advanced", Notes = "Strong in STEM subjects" },
                
                // Lucas Wright (Grade 9) - Struggling student
                new StudentProgress { Id = 11, StudentId = 16, SubjectId = 9, AssessmentDate = new DateTime(2025, 1, 10), CompletionPercentage = 42.0m, PerformanceScore = 48.0m, ProgressLevel = "Below Basic", Notes = "Requires intensive support" },
                new StudentProgress { Id = 12, StudentId = 16, SubjectId = 10, AssessmentDate = new DateTime(2025, 1, 12), CompletionPercentage = 58.0m, PerformanceScore = 62.0m, ProgressLevel = "Basic", Notes = "Some improvement in writing" },
                
                // Harper Lopez (Grade 9) - Sporadic attendance
                new StudentProgress { Id = 13, StudentId = 17, SubjectId = 9, AssessmentDate = new DateTime(2024, 12, 15), CompletionPercentage = 35.0m, PerformanceScore = 40.0m, ProgressLevel = "Below Basic", Notes = "Attendance issues affecting progress" },
                
                // Abigail Baker (Grade 12) - Valedictorian candidate
                new StudentProgress { Id = 14, StudentId = 21, SubjectId = 9, AssessmentDate = new DateTime(2025, 2, 1), CompletionPercentage = 99.0m, PerformanceScore = 98.0m, ProgressLevel = "Advanced", Notes = "Outstanding academic performance" },
                new StudentProgress { Id = 15, StudentId = 21, SubjectId = 10, AssessmentDate = new DateTime(2025, 2, 5), CompletionPercentage = 97.0m, PerformanceScore = 96.0m, ProgressLevel = "Advanced", Notes = "Exceptional writing and analysis skills" }
            );

            // Seed Student Assignments (with varied completion and scores)
            modelBuilder.Entity<StudentAssignment>().HasData(
                // Emma Wilson assignments (Advanced student)
                new StudentAssignment { Id = 1, StudentId = 1, AssignmentId = 1, SubmissionDate = new DateTime(2025, 1, 14), Score = 19.0m, Status = "Graded", TeacherFeedback = "Excellent work! Keep it up!" },
                new StudentAssignment { Id = 2, StudentId = 1, AssignmentId = 2, SubmissionDate = new DateTime(2025, 1, 17), Score = 10.0m, Status = "Graded", TeacherFeedback = "Perfect sight word recognition" },
                
                // Liam Anderson assignments (Struggling student)
                new StudentAssignment { Id = 3, StudentId = 2, AssignmentId = 1, SubmissionDate = new DateTime(2025, 1, 16), Score = 12.0m, Status = "Graded", TeacherFeedback = "Good effort, practice numbers 15-20 more" },
                new StudentAssignment { Id = 4, StudentId = 2, AssignmentId = 2, SubmissionDate = null, Score = null, Status = "NotStarted", TeacherFeedback = "Please complete and turn in" },
                
                // Ava Lee assignments (Math whiz)
                new StudentAssignment { Id = 5, StudentId = 7, AssignmentId = 4, SubmissionDate = new DateTime(2025, 1, 31), Score = 30.0m, Status = "Graded", TeacherFeedback = "Perfect understanding of fractions!" },
                new StudentAssignment { Id = 6, StudentId = 7, AssignmentId = 6, SubmissionDate = new DateTime(2025, 2, 18), Score = 38.0m, Status = "Graded", TeacherFeedback = "Creative and accurate model" },
                
                // Mason White assignments (Reading struggles)
                new StudentAssignment { Id = 7, StudentId = 8, AssignmentId = 4, SubmissionDate = new DateTime(2025, 2, 2), Score = 22.0m, Status = "Graded", TeacherFeedback = "Good math work, keep practicing" },
                new StudentAssignment { Id = 8, StudentId = 8, AssignmentId = 5, SubmissionDate = new DateTime(2025, 2, 16), Score = 32.0m, Status = "Graded", TeacherFeedback = "Nice improvement in reading comprehension" },
                
                // Benjamin Young assignments (Advanced student)
                new StudentAssignment { Id = 9, StudentId = 14, AssignmentId = 7, SubmissionDate = new DateTime(2025, 3, 4), Score = 96.0m, Status = "Graded", TeacherFeedback = "Exceptional understanding of quadratics" },
                new StudentAssignment { Id = 10, StudentId = 14, AssignmentId = 9, SubmissionDate = new DateTime(2025, 3, 14), Score = 72.0m, Status = "Graded", TeacherFeedback = "Good lab technique and observations" },
                
                // Lucas Wright assignments (Struggling student)
                new StudentAssignment { Id = 11, StudentId = 16, AssignmentId = 7, SubmissionDate = new DateTime(2025, 3, 6), Score = 45.0m, Status = "Graded", TeacherFeedback = "Needs more practice with quadratic formula" },
                new StudentAssignment { Id = 12, StudentId = 16, AssignmentId = 8, SubmissionDate = null, Score = null, Status = "InProgress", TeacherFeedback = "Working on essay draft" },
                
                // Harper Lopez assignments (Sporadic student)
                new StudentAssignment { Id = 13, StudentId = 17, AssignmentId = 7, SubmissionDate = null, Score = null, Status = "NotStarted", TeacherFeedback = "Please see me about makeup work" },
                
                // Abigail Baker assignments (Valedictorian candidate)
                new StudentAssignment { Id = 14, StudentId = 21, AssignmentId = 7, SubmissionDate = new DateTime(2025, 3, 3), Score = 100.0m, Status = "Graded", TeacherFeedback = "Outstanding work as always!" },
                new StudentAssignment { Id = 15, StudentId = 21, AssignmentId = 8, SubmissionDate = new DateTime(2025, 3, 9), Score = 98.0m, Status = "Graded", TeacherFeedback = "Insightful analysis and excellent writing" }
            );
        }

        /// <summary>
        /// Seeds the database with initial data including roles and users with proper Identity setup
        /// </summary>
        public static async Task SeedDataAsync(
            ApplicationDbContext context, 
            UserManager<User> userManager, 
            RoleManager<IdentityRole<int>> roleManager, 
            ILogger logger)
        {
            try
            {
                // Ensure roles exist
                string[] roles = { "Admin", "Teacher", "Student" };
                
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                        logger.LogInformation("Created role: {Role}", role);
                    }
                }

                // Create admin user if it doesn't exist
                var adminEmail = "admin@studenttracker.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                
                if (adminUser == null)
                {                adminUser = new User
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = adminEmail,
                    UserName = adminEmail,
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Created admin user: {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Create sample teacher if it doesn't exist
                var teacherEmail = "teacher@studenttracker.com";
                var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
                
                if (teacherUser == null)
                {                teacherUser = new User
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Email = teacherEmail,
                    UserName = teacherEmail,
                    Role = "Teacher",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                    var result = await userManager.CreateAsync(teacherUser, "Teacher123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(teacherUser, "Teacher");
                        logger.LogInformation("Created teacher user: {Email}", teacherEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create teacher user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Create sample student if it doesn't exist
                var studentEmail = "student@studenttracker.com";
                var studentUser = await userManager.FindByEmailAsync(studentEmail);
                
                if (studentUser == null)
                {                studentUser = new User
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = studentEmail,
                    UserName = studentEmail,
                    Role = "Student",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                    var result = await userManager.CreateAsync(studentUser, "Student123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(studentUser, "Student");
                        logger.LogInformation("Created student user: {Email}", studentEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create student user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Student || e.Entity is Subject || e.Entity is StudentProgress || 
                           e.Entity is Assignment || e.Entity is StudentAssignment || e.Entity is User);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}
