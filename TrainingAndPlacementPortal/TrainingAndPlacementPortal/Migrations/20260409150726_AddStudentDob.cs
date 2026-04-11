using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingAndPlacementPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentDob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$oAIf6y4.3xl3Z9TsV6avPOzrHHIptxYWAEMCCoDbVLIPSLW8cTnoy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Students");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$BJ8RgUvBMKSrKvuOo2MK6.pIobJJxdeGWPZ3Hqdd0YdjM30jhfk4G");
        }
    }
}
