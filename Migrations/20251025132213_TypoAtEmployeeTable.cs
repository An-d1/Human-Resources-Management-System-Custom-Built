using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class TypoAtEmployeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isEmployed",
                table: "Employees",
                newName: "IsEmployed");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsEmployed",
                table: "Employees",
                newName: "isEmployed");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Companies",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
