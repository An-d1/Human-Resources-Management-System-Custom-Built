using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsActiveAtCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Companies",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Companies");
        }
    }
}
