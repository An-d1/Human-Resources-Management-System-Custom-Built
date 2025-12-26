using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmployeLevelIdToEMployeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalculatedDays",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeLevelId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeLevelId",
                table: "Employees",
                column: "EmployeeLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeesLevel_EmployeeLevelId",
                table: "Employees",
                column: "EmployeeLevelId",
                principalTable: "EmployeesLevel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeesLevel_EmployeeLevelId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeLevelId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CalculatedDays",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "EmployeeLevelId",
                table: "Employees");
        }
    }
}
