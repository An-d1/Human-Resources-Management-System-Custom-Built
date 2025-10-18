using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCompanyModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhoneNumber",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyPhoneNumber",
                table: "Companies");
        }
    }
}
