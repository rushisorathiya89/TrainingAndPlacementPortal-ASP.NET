using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingAndPlacementPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<string>(
                name: "LocationOrLink",
                table: "InterviewSchedules",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "InterviewType",
                table: "InterviewSchedules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

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
                name: "Status",
                table: "InterviewSchedules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WaitingArea",
                table: "InterviewSchedules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");


            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$BJ8RgUvBMKSrKvuOo2MK6.pIobJJxdeGWPZ3Hqdd0YdjM30jhfk4G");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "RoundName",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "WaitingArea",
                table: "InterviewSchedules");


            migrationBuilder.AlterColumn<string>(
                name: "LocationOrLink",
                table: "InterviewSchedules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "InterviewType",
                table: "InterviewSchedules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$TmWiW5KXuATt/CEDlfSJfu.CdP6feIPqKtYLjYujRYA3.tN1WhVZC");
        }
    }
}
