using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingAndPlacementPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddJdApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MinCGPA",
                table: "JobPostings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "JobPostings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdminRemarks",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$1RNFCTeMy5dnx9kabrxO/O5GGjs.1pmxyXFNGfdlbC1HlVbbpE0cy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinCGPA",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "AdminRemarks",
                table: "Companies");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$GdIUjEar/o7zd1HnXw40DOyGLjbVu1mkCLY1laWAxRd4hEGc2qc/K");
        }
    }
}
