using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AllowPayRateHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayRates_EmployeeId",
                table: "PayRates");

            migrationBuilder.CreateIndex(
                name: "IX_PayRates_EmployeeId",
                table: "PayRates",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayRates_EmployeeId",
                table: "PayRates");

            migrationBuilder.CreateIndex(
                name: "IX_PayRates_EmployeeId",
                table: "PayRates",
                column: "EmployeeId",
                unique: true);
        }
    }
}
