using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudentProgressTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Grade = table.Column<int>(type: "INTEGER", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Grade = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "TEXT", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaxPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignmentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PerformanceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TimeSpentMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProgressLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    LastActivityDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgress_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgress_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherStudents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnassignedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherStudents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherStudents_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TeacherFeedback = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TimeSpentMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAssignments_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAssignments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "CreatedAt", "DateOfBirth", "Email", "EnrollmentDate", "FirstName", "Grade", "IsActive", "LastName", "Notes", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(2501), new DateTime(2018, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "emma.wilson@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Emma", 1, true, "Wilson", "Enthusiastic learner, loves reading", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(2510) },
                    { 2, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5433), new DateTime(2018, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "liam.anderson@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Liam", 1, true, "Anderson", "Struggles with math concepts, needs extra support", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5440) },
                    { 3, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5464), new DateTime(2018, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "sophia.martinez@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sophia", 1, true, "Martinez", "Advanced reader, quiet in class", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5465) },
                    { 4, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5466), new DateTime(2016, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "noah.thompson@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Noah", 3, true, "Thompson", "Good at problem solving, active participant", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5467) },
                    { 5, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5469), new DateTime(2016, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "olivia.garcia@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Olivia", 3, true, "Garcia", "Excellent in science, curious about nature", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5470) },
                    { 6, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5471), new DateTime(2016, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "ethan.rodriguez@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ethan", 3, true, "Rodriguez", "Sporadic attendance, needs encouragement", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5471) },
                    { 7, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5473), new DateTime(2014, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "ava.lee@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ava", 5, true, "Lee", "Math whiz, helps other students", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5474) },
                    { 8, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5475), new DateTime(2014, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "mason.white@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mason", 5, true, "White", "Struggles with reading comprehension", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5476) },
                    { 9, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5477), new DateTime(2014, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "isabella.harris@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isabella", 5, true, "Harris", "Well-rounded student, consistent performer", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5478) },
                    { 10, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5479), new DateTime(2014, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "william.clark@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "William", 5, true, "Clark", "Creative writer, loves social studies", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5480) },
                    { 11, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5606), new DateTime(2012, 6, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "mia.lewis@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mia", 7, true, "Lewis", "Strong in all subjects, leadership qualities", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5606) },
                    { 12, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5608), new DateTime(2012, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "james.walker@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "James", 7, true, "Walker", "Inactive lately, needs motivation", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5609) },
                    { 13, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5611), new DateTime(2012, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "charlotte.hall@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Charlotte", 7, true, "Hall", "Excels in science, future engineer", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5611) },
                    { 14, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5661), new DateTime(2010, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "benjamin.young@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Benjamin", 9, true, "Young", "Advanced in mathematics, peer tutor", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5668) },
                    { 15, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5670), new DateTime(2010, 1, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "amelia.king@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amelia", 9, true, "King", "Gifted writer, participates in debate team", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5670) },
                    { 16, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5672), new DateTime(2010, 5, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "lucas.wright@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lucas", 9, true, "Wright", "Struggling student, requires additional support", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5673) },
                    { 17, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5674), new DateTime(2010, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "harper.lopez@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Harper", 9, true, "Lopez", "Inconsistent attendance, family issues", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5675) },
                    { 18, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5676), new DateTime(2008, 11, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "alexander.hill@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Alexander", 11, true, "Hill", "AP student, college prep track", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5677) },
                    { 19, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5678), new DateTime(2008, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "evelyn.green@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Evelyn", 11, true, "Green", "Strong in humanities, weak in STEM", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5678) },
                    { 20, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5680), new DateTime(2008, 8, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "henry.adams@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Henry", 11, true, "Adams", "Athlete, needs academic balance", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5680) },
                    { 21, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5682), new DateTime(2007, 2, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "abigail.baker@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Abigail", 12, true, "Baker", "Valedictorian candidate, exceptional student", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5682) },
                    { 22, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5684), new DateTime(2007, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "michael.nelson@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Michael", 12, true, "Nelson", "Late bloomer, improving rapidly", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5684) },
                    { 23, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5697), new DateTime(2007, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "emily.carter@school.edu", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Emily", 12, true, "Carter", "Art-focused student, creative thinker", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5698) }
                });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "Grade", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "MATH", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(381), "Basic arithmetic and number concepts", 1, true, "Mathematics", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(387) },
                    { 2, "READ", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1106), "Phonics, sight words, and comprehension", 1, true, "Reading", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1106) },
                    { 3, "SCI", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1109), "Nature, simple experiments, and observations", 1, true, "Science", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1109) },
                    { 4, "SS", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1110), "Community, family, and basic geography", 1, true, "Social Studies", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1110) },
                    { 5, "MATH", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1163), "Fractions, decimals, and basic algebra", 5, true, "Mathematics", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1163) },
                    { 6, "READ", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1164), "Advanced comprehension and writing skills", 5, true, "Reading", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1165) },
                    { 7, "SCI", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1166), "Earth science, biology, and scientific method", 5, true, "Science", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1166) },
                    { 8, "SS", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1167), "US history, geography, and civics", 5, true, "Social Studies", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1167) },
                    { 9, "MATH", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1168), "Algebra, geometry, and problem solving", 9, true, "Mathematics", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1168) },
                    { 10, "ENG", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1169), "Literature, composition, and critical thinking", 9, true, "English", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1169) },
                    { 11, "SCI", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1170), "Biology, chemistry, and physics concepts", 9, true, "Science", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1171) },
                    { 12, "SS", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1172), "World history and cultural studies", 9, true, "Social Studies", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1172) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsActive", "LastLoginDate", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Role", "SecurityStamp", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { 1, 0, "c8be7825-a563-4e23-b21f-04c24fddc7fa", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(5980), "admin@edtech.com", false, "Admin", true, null, "User", false, null, null, null, "$2a$11$9k/pu74kKbgnChnI2yIvm.Z8fPcHRTGZwzmdaKm8hpNDImlwDAmoq", null, false, "Admin", null, false, new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(5983), null },
                    { 2, 0, "e6e8778c-755e-4069-a1c1-5404568c5591", new DateTime(2025, 7, 11, 13, 35, 21, 274, DateTimeKind.Utc).AddTicks(221), "sarah.johnson@edtech.com", false, "Sarah", true, null, "Johnson", false, null, null, null, "$2a$11$oh46SD8eYyiyLW2y7pkUyOJhrT92ybnJB5MStQ9vS9.yqXacZ.Sem", null, false, "Teacher", null, false, new DateTime(2025, 7, 11, 13, 35, 21, 274, DateTimeKind.Utc).AddTicks(228), null },
                    { 3, 0, "2b03f7e0-0d19-4ba5-9468-2ad63c85adad", new DateTime(2025, 7, 11, 13, 35, 21, 435, DateTimeKind.Utc).AddTicks(862), "michael.brown@edtech.com", false, "Michael", true, null, "Brown", false, null, null, null, "$2a$11$krzcM1FaKkbr8fbC1aW3UexbWl5wYB1ExNbaF91XGiZmJF41w83pm", null, false, "Teacher", null, false, new DateTime(2025, 7, 11, 13, 35, 21, 435, DateTimeKind.Utc).AddTicks(872), null },
                    { 4, 0, "d43621ef-7d98-4f40-8d72-9b13eecf71c8", new DateTime(2025, 7, 11, 13, 35, 21, 574, DateTimeKind.Utc).AddTicks(6385), "jennifer.davis@edtech.com", false, "Jennifer", true, null, "Davis", false, null, null, null, "$2a$11$ftvGzn2tlbLqxuWk5bDIWO/wYIxnidcSIDhdt3j6aHZPhKtuvervS", null, false, "Teacher", null, false, new DateTime(2025, 7, 11, 13, 35, 21, 574, DateTimeKind.Utc).AddTicks(6392), null }
                });

            migrationBuilder.InsertData(
                table: "Assignments",
                columns: new[] { "Id", "AssignmentType", "CreatedAt", "Description", "DueDate", "IsActive", "MaxPoints", "SubjectId", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Practice", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7112), "Identify and write numbers 1-20", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 20, 1, "Number Recognition 1-20", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7113) },
                    { 2, "Quiz", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7735), "Learn first 10 sight words", new DateTime(2025, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 10, 2, "Sight Words Week 1", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7735) },
                    { 3, "Project", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7738), "Daily weather tracking for one week", new DateTime(2025, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 25, 3, "Weather Observation", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7738) },
                    { 4, "Homework", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7739), "Adding fractions with like denominators", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 30, 5, "Fraction Addition", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7740) },
                    { 5, "Project", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7741), "Complete book report with analysis", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 50, 6, "Book Report: Charlotte's Web", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7742) },
                    { 6, "Project", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7762), "Create 3D model of solar system", new DateTime(2025, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 40, 7, "Solar System Model", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7762) },
                    { 7, "Test", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7764), "Solving quadratic equations unit test", new DateTime(2025, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 100, 9, "Quadratic Equations Test", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7764) },
                    { 8, "Essay", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7765), "5-paragraph essay on themes in Romeo and Juliet", new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 100, 10, "Romeo and Juliet Essay", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7765) },
                    { 9, "Lab", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7766), "Microscope lab on cell structures", new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 75, 11, "Cell Structure Lab", new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7767) }
                });

            migrationBuilder.InsertData(
                table: "StudentProgress",
                columns: new[] { "Id", "AssessmentDate", "CompletionPercentage", "CreatedAt", "LastActivityDate", "Notes", "PerformanceScore", "ProgressLevel", "StudentId", "SubjectId", "TimeSpentMinutes", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 95.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9083), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Excellent number recognition skills", 92.0m, "Advanced", 1, 1, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9084) },
                    { 2, new DateTime(2024, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 88.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9902), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reading above grade level", 90.0m, "Advanced", 1, 2, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9902) },
                    { 3, new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 45.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9904), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Needs additional math support", 52.0m, "Below Basic", 2, 1, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9904) },
                    { 4, new DateTime(2024, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 60.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9906), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Showing improvement with phonics", 65.0m, "Basic", 2, 2, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9906) },
                    { 5, new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 98.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9908), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exceptional mathematical reasoning", 97.0m, "Advanced", 7, 5, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9908) },
                    { 6, new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 85.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9909), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Strong science understanding", 88.0m, "Proficient", 7, 7, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9909) },
                    { 7, new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 55.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9911), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reading comprehension needs work", 58.0m, "Basic", 8, 6, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9911) },
                    { 8, new DateTime(2024, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 72.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9947), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Better with math calculations", 75.0m, "Proficient", 8, 5, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9947) },
                    { 9, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 96.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9948), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Consistently high performance", 94.0m, "Advanced", 14, 9, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9949) },
                    { 10, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 89.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9959), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Strong in STEM subjects", 91.0m, "Advanced", 14, 11, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9960) },
                    { 11, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 42.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9961), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Requires intensive support", 48.0m, "Below Basic", 16, 9, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9962) },
                    { 12, new DateTime(2025, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 58.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9963), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Some improvement in writing", 62.0m, "Basic", 16, 10, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9963) },
                    { 13, new DateTime(2024, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 35.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9965), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Attendance issues affecting progress", 40.0m, "Below Basic", 17, 9, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9965) },
                    { 14, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 99.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9966), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Outstanding academic performance", 98.0m, "Advanced", 21, 9, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9967) },
                    { 15, new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 97.0m, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9968), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exceptional writing and analysis skills", 96.0m, "Advanced", 21, 10, 0, new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9968) }
                });

            migrationBuilder.InsertData(
                table: "TeacherStudents",
                columns: new[] { "Id", "AssignedDate", "CreatedAt", "IsActive", "StudentId", "TeacherId", "UnassignedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8217), true, 1, 2, null },
                    { 2, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8602), true, 2, 2, null },
                    { 3, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8603), true, 3, 2, null },
                    { 4, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8605), true, 7, 3, null },
                    { 5, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8605), true, 8, 3, null },
                    { 6, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8606), true, 9, 3, null },
                    { 7, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8607), true, 10, 3, null },
                    { 8, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8608), true, 14, 4, null },
                    { 9, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8609), true, 15, 4, null },
                    { 10, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8610), true, 16, 4, null },
                    { 11, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8611), true, 18, 4, null },
                    { 12, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8612), true, 21, 4, null }
                });

            migrationBuilder.InsertData(
                table: "StudentAssignments",
                columns: new[] { "Id", "AssignmentId", "CreatedAt", "Score", "Status", "StudentId", "SubmissionDate", "TeacherFeedback", "TimeSpentMinutes", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(300), 19.0m, "Graded", 1, new DateTime(2025, 1, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Excellent work! Keep it up!", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(301) },
                    { 2, 2, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1066), 10.0m, "Graded", 1, new DateTime(2025, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Perfect sight word recognition", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1066) },
                    { 3, 1, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1069), 12.0m, "Graded", 2, new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good effort, practice numbers 15-20 more", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1069) },
                    { 4, 2, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1084), null, "NotStarted", 2, null, "Please complete and turn in", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1084) },
                    { 5, 4, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1085), 30.0m, "Graded", 7, new DateTime(2025, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Perfect understanding of fractions!", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1085) },
                    { 6, 6, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1087), 38.0m, "Graded", 7, new DateTime(2025, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Creative and accurate model", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1087) },
                    { 7, 4, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1088), 22.0m, "Graded", 8, new DateTime(2025, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good math work, keep practicing", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1089) },
                    { 8, 5, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1090), 32.0m, "Graded", 8, new DateTime(2025, 2, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nice improvement in reading comprehension", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1090) },
                    { 9, 7, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1092), 96.0m, "Graded", 14, new DateTime(2025, 3, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exceptional understanding of quadratics", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1092) },
                    { 10, 9, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1093), 72.0m, "Graded", 14, new DateTime(2025, 3, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good lab technique and observations", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1094) },
                    { 11, 7, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1095), 45.0m, "Graded", 16, new DateTime(2025, 3, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Needs more practice with quadratic formula", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1096) },
                    { 12, 8, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1097), null, "InProgress", 16, null, "Working on essay draft", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1097) },
                    { 13, 7, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1098), null, "NotStarted", 17, null, "Please see me about makeup work", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1098) },
                    { 14, 7, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1099), 100.0m, "Graded", 21, new DateTime(2025, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Outstanding work as always!", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1100) },
                    { 15, 8, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1101), 98.0m, "Graded", 21, new DateTime(2025, 3, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Insightful analysis and excellent writing", 0, new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1101) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_SubjectId",
                table: "Assignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignments_AssignmentId",
                table: "StudentAssignments",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignments_StudentId_AssignmentId",
                table: "StudentAssignments",
                columns: new[] { "StudentId", "AssignmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_StudentId_SubjectId_AssessmentDate",
                table: "StudentProgress",
                columns: new[] { "StudentId", "SubjectId", "AssessmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_SubjectId",
                table: "StudentProgress",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code",
                table: "Subjects",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherStudents_StudentId",
                table: "TeacherStudents",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherStudents_TeacherId_StudentId_IsActive",
                table: "TeacherStudents",
                columns: new[] { "TeacherId", "StudentId", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAssignments");

            migrationBuilder.DropTable(
                name: "StudentProgress");

            migrationBuilder.DropTable(
                name: "TeacherStudents");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
