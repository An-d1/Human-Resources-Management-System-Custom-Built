using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddedPayRateToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PayRateId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PayRates_PayRateId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PayRateId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PayRateId",
                table: "Employees");
        }
    }
}
