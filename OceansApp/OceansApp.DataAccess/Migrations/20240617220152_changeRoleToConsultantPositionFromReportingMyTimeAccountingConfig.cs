using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class changeRoleToConsultantPositionFromReportingMyTimeAccountingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_Roles_RoleId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_RoleId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION");

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_PositionId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_CONSULTANT_POSITIONS_PositionId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "PositionId",
                principalTable: "CONSULTANT_POSITIONS",
                principalColumn: "ConsultantPositionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_CONSULTANT_POSITIONS_PositionId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_PositionId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION");

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_RoleId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_Roles_RoleId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
