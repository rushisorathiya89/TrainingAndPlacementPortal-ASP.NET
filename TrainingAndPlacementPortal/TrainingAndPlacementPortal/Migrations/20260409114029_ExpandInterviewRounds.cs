using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingAndPlacementPortal.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInterviewRounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobLocation",
                table: "JobPostings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoundName",
                table: "InterviewSchedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "InterviewSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Timing",
                table: "InterviewSchedules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "InterviewSchedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WaitingArea",
                table: "InterviewSchedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$DmVgzahhAP8lvvinePmmHe6uSV3yH0QESq7FzN8rm7q0Ukr.nUsJC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobLocation",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "RoundName",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "Timing",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "WaitingArea",
                table: "InterviewSchedules");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$1RNFCTeMy5dnx9kabrxO/O5GGjs.1pmxyXFNGfdlbC1HlVbbpE0cy");
        }
    }
}
