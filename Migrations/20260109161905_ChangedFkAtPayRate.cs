using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ChangedFkAtPayRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PayRates_PayRateId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_PayRates_AspNetUsers_UserId",
                table: "PayRates");

            migrationBuilder.DropIndex(
                name: "IX_PayRates_UserId",
                table: "PayRates");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PayRateId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PayRateId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PayRates",
                newName: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayRates_EmployeeId",
                table: "PayRates",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PayRates_Employees_EmployeeId",
                table: "PayRates",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayRates_Employees_EmployeeId",
                table: "PayRates");

            migrationBuilder.DropIndex(
                name: "IX_PayRates_EmployeeId",
                table: "PayRates");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "PayRates",
                newName: "UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "PayRateId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayRates_UserId",
                table: "PayRates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PayRateId",
                table: "Employees",
                column: "PayRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PayRates_PayRateId",
                table: "Employees",
                column: "PayRateId",
                principalTable: "PayRates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PayRates_AspNetUsers_UserId",
                table: "PayRates",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
