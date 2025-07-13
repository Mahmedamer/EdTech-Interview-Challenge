using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProgressTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherStudents_Users_TeacherId",
                table: "TeacherStudents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "AspNetUsers");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "ece5de07-1999-42f1-b00a-eeda1086d1aa", new DateTime(2025, 7, 11, 13, 56, 12, 739, DateTimeKind.Utc).AddTicks(640), "$2a$11$S36EcoDUOFo.vuQ0Zj6PMuk7854Ss8XvGqhDppqcO72Vy0t0dDK/S", new DateTime(2025, 7, 11, 13, 56, 12, 739, DateTimeKind.Utc).AddTicks(643) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "fb0bc439-bb84-4b1b-920e-aee06467046c", new DateTime(2025, 7, 11, 13, 56, 12, 921, DateTimeKind.Utc).AddTicks(1587), "$2a$11$GMCid319nWv6kf60O45./.QcCyQ2gcbrDSIdUgRB.mBvtRNd5Dfui", new DateTime(2025, 7, 11, 13, 56, 12, 921, DateTimeKind.Utc).AddTicks(1595) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "c33cb726-8bbb-47fe-8ff8-e30ee3ed2b57", new DateTime(2025, 7, 11, 13, 56, 13, 87, DateTimeKind.Utc).AddTicks(4796), "$2a$11$n2POrepGuYCN8VBdeVFqbe9xSaD0V3HcaFT0hbULt/g.LU/lPMdYi", new DateTime(2025, 7, 11, 13, 56, 13, 87, DateTimeKind.Utc).AddTicks(4804) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "b2671a54-aff7-4a2b-8b32-f6822880189d", new DateTime(2025, 7, 11, 13, 56, 13, 219, DateTimeKind.Utc).AddTicks(9195), "$2a$11$iTlTXQp1ykhJIGDrjSt2aeforXBEuQmGXqCe4q6UBwTBHx5I4SGFa", new DateTime(2025, 7, 11, 13, 56, 13, 219, DateTimeKind.Utc).AddTicks(9202) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(5743), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(5744) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6424), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6425) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6442), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6442) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6444), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6444) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6445), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6446) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6447), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6448) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6449), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6449) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6450), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6451) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6452), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6452) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8741), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8742) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9516), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9516) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9519), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9519) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9521), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9521) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9522), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9522) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9524), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9524) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9525), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9526) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9527), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9527) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9529), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9529) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9530), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9531) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9532), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9532) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9534), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9534) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9535), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9535) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9536), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9536) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9538), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(9538) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7710), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7710) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8387), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8388) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8390), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8390) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8392), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8392) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8394), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8394) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8395), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8396) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8397), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8397) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8399), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8399) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8416), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8416) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8418), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8418) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8420), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8420) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8422), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8422) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8423), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8423) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8425), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8425) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8427), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(8427) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(3659), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(3665) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4778), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4779) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4783), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4783) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4787), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4787) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4790), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4790) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4791), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4792) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4794), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4794) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4795), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4796) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4797), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4798) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4807), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4807) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4808), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4808) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4836), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4840) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4841), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4841) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4843), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4843) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4845), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4845) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4846), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4846) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4848), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4848) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4849), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4849) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4856), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4856) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4858), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4858) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4859), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4859) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4861), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4861) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4862), new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(4862) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5198), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5202) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5891), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5891) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5894), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5894) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5896), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5896) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5897), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5897) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5898), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5899) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5900), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5900) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5901), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5901) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5902), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5902) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5904), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5904) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5943), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5943) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5944), new DateTime(2025, 7, 11, 13, 56, 12, 738, DateTimeKind.Utc).AddTicks(5944) });

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(6847));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7279));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7281));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7321));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7322));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7323));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7324));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7325));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7326));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7327));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7341));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 56, 13, 330, DateTimeKind.Utc).AddTicks(7342));

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherStudents_AspNetUsers_TeacherId",
                table: "TeacherStudents",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherStudents_AspNetUsers_TeacherId",
                table: "TeacherStudents");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7112), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7113) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7735), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7735) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7738), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7738) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7739), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7740) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7741), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7742) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7762), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7762) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7764), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7764) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7765), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7765) });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7766), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(7767) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(300), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(301) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1066), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1066) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1069), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1069) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1084), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1084) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1085), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1085) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1087), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1087) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1088), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1089) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1090), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1090) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1092), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1092) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1093), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1094) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1095), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1096) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1097), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1097) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1098), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1098) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1099), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1100) });

            migrationBuilder.UpdateData(
                table: "StudentAssignments",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1101), new DateTime(2025, 7, 11, 13, 35, 21, 686, DateTimeKind.Utc).AddTicks(1101) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9083), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9084) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9902), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9902) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9904), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9904) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9906), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9906) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9908), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9908) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9909), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9909) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9911), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9911) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9947), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9947) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9948), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9949) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9959), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9960) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9961), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9962) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9963), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9963) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9965), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9965) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9966), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9967) });

            migrationBuilder.UpdateData(
                table: "StudentProgress",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9968), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(9968) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(2501), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(2510) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5433), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5440) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5464), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5465) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5466), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5467) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5469), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5470) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5471), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5471) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5473), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5474) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5475), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5476) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5477), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5478) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5479), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5480) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5606), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5606) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5608), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5609) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5611), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5611) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5661), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5668) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5670), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5670) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5672), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5673) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5674), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5675) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5676), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5677) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5678), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5678) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5680), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5680) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5682), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5682) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5684), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5684) });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5697), new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(5698) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(381), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(387) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1106), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1106) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1109), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1109) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1110), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1110) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1163), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1163) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1164), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1165) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1166), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1166) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1167), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1167) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1168), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1168) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1169), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1169) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1170), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1171) });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1172), new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(1172) });

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8217));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8602));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8603));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8605));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8605));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8606));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8607));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8608));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8609));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8610));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8611));

            migrationBuilder.UpdateData(
                table: "TeacherStudents",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 11, 13, 35, 21, 685, DateTimeKind.Utc).AddTicks(8612));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "c8be7825-a563-4e23-b21f-04c24fddc7fa", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(5980), "$2a$11$9k/pu74kKbgnChnI2yIvm.Z8fPcHRTGZwzmdaKm8hpNDImlwDAmoq", new DateTime(2025, 7, 11, 13, 35, 21, 105, DateTimeKind.Utc).AddTicks(5983) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "e6e8778c-755e-4069-a1c1-5404568c5591", new DateTime(2025, 7, 11, 13, 35, 21, 274, DateTimeKind.Utc).AddTicks(221), "$2a$11$oh46SD8eYyiyLW2y7pkUyOJhrT92ybnJB5MStQ9vS9.yqXacZ.Sem", new DateTime(2025, 7, 11, 13, 35, 21, 274, DateTimeKind.Utc).AddTicks(228) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "2b03f7e0-0d19-4ba5-9468-2ad63c85adad", new DateTime(2025, 7, 11, 13, 35, 21, 435, DateTimeKind.Utc).AddTicks(862), "$2a$11$krzcM1FaKkbr8fbC1aW3UexbWl5wYB1ExNbaF91XGiZmJF41w83pm", new DateTime(2025, 7, 11, 13, 35, 21, 435, DateTimeKind.Utc).AddTicks(872) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "d43621ef-7d98-4f40-8d72-9b13eecf71c8", new DateTime(2025, 7, 11, 13, 35, 21, 574, DateTimeKind.Utc).AddTicks(6385), "$2a$11$ftvGzn2tlbLqxuWk5bDIWO/wYIxnidcSIDhdt3j6aHZPhKtuvervS", new DateTime(2025, 7, 11, 13, 35, 21, 574, DateTimeKind.Utc).AddTicks(6392) });

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherStudents_Users_TeacherId",
                table: "TeacherStudents",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
