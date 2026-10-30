using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leaderboard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddClassTeacherName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassTeacherName",
                table: "SchoolClass",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassTeacherName",
                table: "SchoolClass");
        }
    }
}
