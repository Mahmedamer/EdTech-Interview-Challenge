using AutoMapper;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Student mappings
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => 
                    DateTime.UtcNow.Year - src.DateOfBirth.Year - 
                    (DateTime.UtcNow.DayOfYear < src.DateOfBirth.DayOfYear ? 1 : 0)))
                .ForMember(dest => dest.LastActivityDate, opt => opt.MapFrom(src =>
                    src.ProgressRecords.Any() ? src.ProgressRecords.Max(p => p.LastActivityDate) : src.CreatedAt))
                .ForMember(dest => dest.OverallProgress, opt => opt.MapFrom(src =>
                    src.ProgressRecords.Any() ? src.ProgressRecords.Average(p => p.CompletionPercentage) : 0))
                .ForMember(dest => dest.ProgressLevel, opt => opt.MapFrom(src => 
                    DetermineProgressLevel(src.ProgressRecords)));

            CreateMap<StudentCreateDto, Student>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProgressRecords, opt => opt.Ignore())
                .ForMember(dest => dest.StudentAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherStudents, opt => opt.Ignore());

            // StudentProgress mappings
            CreateMap<StudentProgress, StudentProgressDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => $"{src.Student.FirstName} {src.Student.LastName}"))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name));

            CreateMap<StudentProgressCreateDto, StudentProgress>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastActivityDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.Ignore());

            // User mappings
            CreateMap<User, AuthDto.UserProfileDto>();

            // Teacher mappings
            CreateMap<User, TeacherDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.TotalStudents, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveStudents, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedGrades, opt => opt.Ignore())
                .ForMember(dest => dest.Statistics, opt => opt.Ignore());

            CreateMap<User, TeacherDetailsDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.TotalStudents, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveStudents, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedGrades, opt => opt.Ignore())
                .ForMember(dest => dest.Statistics, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedStudents, opt => opt.Ignore())
                .ForMember(dest => dest.StudentAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.Performance, opt => opt.Ignore());

            CreateMap<TeacherCreateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherStudents, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());

            // TeacherStudent mappings
            CreateMap<TeacherStudentAssignmentCreateDto, TeacherStudent>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UnassignedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore());

            // Subject mappings
            CreateMap<Subject, SubjectDto>()
                .ForMember(dest => dest.TotalAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveStudents, opt => opt.Ignore())
                .ForMember(dest => dest.AverageProgress, opt => opt.Ignore());

            CreateMap<Subject, SubjectDetailsDto>()
                .ForMember(dest => dest.TotalAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveStudents, opt => opt.Ignore())
                .ForMember(dest => dest.AverageProgress, opt => opt.Ignore())
                .ForMember(dest => dest.Assignments, opt => opt.Ignore())
                .ForMember(dest => dest.RecentProgress, opt => opt.Ignore())
                .ForMember(dest => dest.Statistics, opt => opt.Ignore());

            CreateMap<SubjectCreateDto, Subject>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProgressRecords, opt => opt.Ignore())
                .ForMember(dest => dest.Assignments, opt => opt.Ignore());

            // Assignment mappings
            CreateMap<Assignment, AssignmentDto>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name));

            CreateMap<AssignmentCreateDto, Assignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.Ignore())
                .ForMember(dest => dest.StudentAssignments, opt => opt.Ignore());

            // StudentAssignment mappings
            CreateMap<StudentAssignment, StudentAssignmentDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => $"{src.Student.FirstName} {src.Student.LastName}"))
                .ForMember(dest => dest.AssignmentTitle, opt => opt.MapFrom(src => src.Assignment.Title));

            CreateMap<StudentAssignmentCreateDto, StudentAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Assignment, opt => opt.Ignore());
        }

        private string DetermineProgressLevel(ICollection<StudentProgress> progressRecords)
        {
            if (!progressRecords.Any()) return "New";

            var averagePerformance = progressRecords.Average(p => p.PerformanceScore);
            var averageCompletion = progressRecords.Average(p => p.CompletionPercentage);

            if (averagePerformance >= 85 && averageCompletion >= 90)
                return "Advanced";
            else if (averagePerformance >= 70 && averageCompletion >= 70)
                return "OnTrack";
            else
                return "Struggling";
        }
    }
}
