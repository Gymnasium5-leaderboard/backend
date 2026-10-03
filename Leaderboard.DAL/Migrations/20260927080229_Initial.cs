using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Leaderboard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicYear",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYear", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardOwner",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'2', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Login = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardOwner", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolClass",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    Letter = table.Column<char>(type: "character(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolClass", x => x.Id);
                    table.CheckConstraint("CK_SchoolClasses_Grade", "\"Grade\" BETWEEN 4 AND 11");
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_LeaderboardOwner_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "LeaderboardOwner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClassId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_SchoolClass_ClassId",
                        column: x => x.ClassId,
                        principalTable: "SchoolClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScoreTransaction",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYearId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Delta = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreTransaction", x => x.Id);
                    table.CheckConstraint("CK_ScoreTransactions_Delta", "\"Delta\" <> 0");
                    table.ForeignKey(
                        name: "FK_ScoreTransaction_AcademicYear_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScoreTransaction_LeaderboardOwner_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "LeaderboardOwner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScoreTransaction_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LeaderboardOwner",
                columns: new[] { "Id", "CreatedAt", "FirstName", "LastModifiedAt", "LastName", "Login", "PasswordHash" },
                values: new object[] { 1L, new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admin", null, "Admin", "admin", "AQAAAAIAAYagAAAAEAS66X8L+GKN2U8Z/GrfEi57uN2ZGbLjTP3uwqL/UwAGiZD8q5g26dXt4dkokFQc5A==" });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYear_Title",
                table: "AcademicYear",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardOwner_Login",
                table: "LeaderboardOwner",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_OwnerId",
                table: "RefreshToken",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolClass_Grade_Letter",
                table: "SchoolClass",
                columns: new[] { "Grade", "Letter" },
                unique: true,
                filter: "\"IsActive\"");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransaction_AcademicYearId_StudentId",
                table: "ScoreTransaction",
                columns: new[] { "AcademicYearId", "StudentId" })
                .Annotation("Npgsql:IndexInclude", new[] { "Delta", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransaction_IdempotencyKey",
                table: "ScoreTransaction",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransaction_OwnerId",
                table: "ScoreTransaction",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransaction_StudentId",
                table: "ScoreTransaction",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_ClassId",
                table: "Student",
                column: "ClassId");

            // Only one current (not finished) academic year
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_AcademicYear_Current\" ON \"AcademicYear\" ((true)) WHERE \"FinishedAt\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IX_AcademicYear_Current\";");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "ScoreTransaction");

            migrationBuilder.DropTable(
                name: "AcademicYear");

            migrationBuilder.DropTable(
                name: "LeaderboardOwner");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "SchoolClass");
        }
    }
}
