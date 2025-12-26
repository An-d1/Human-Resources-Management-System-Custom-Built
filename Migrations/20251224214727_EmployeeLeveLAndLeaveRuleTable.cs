using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeLeveLAndLeaveRuleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeLevelId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeesLevel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeesLevel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeesLevel_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxAnnualLeaves = table.Column<int>(type: "int", nullable: false),
                    ExcludedDays = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRules_EmployeesLevel_EmployeeLevelId",
                        column: x => x.EmployeeLevelId,
                        principalTable: "EmployeesLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmployeeLevelId",
                table: "AspNetUsers",
                column: "EmployeeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesLevel_CompanyId",
                table: "EmployeesLevel",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRules_EmployeeLevelId",
                table: "LeaveRules",
                column: "EmployeeLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_EmployeesLevel_EmployeeLevelId",
                table: "AspNetUsers",
                column: "EmployeeLevelId",
                principalTable: "EmployeesLevel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_EmployeesLevel_EmployeeLevelId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "LeaveRules");

            migrationBuilder.DropTable(
                name: "EmployeesLevel");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EmployeeLevelId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmployeeLevelId",
                table: "AspNetUsers");
        }
    }
}
