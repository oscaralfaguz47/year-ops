using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addUserIdUpdatedByColumnToReportingMyTimeMovementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserIdLastUpdatedBy",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_UserIdLastUpdatedBy",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "UserIdLastUpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_REPORTING_MY_TIME_MOVEMENTS_Users_UserIdLastUpdatedBy",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "UserIdLastUpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_REPORTING_MY_TIME_MOVEMENTS_Users_UserIdLastUpdatedBy",
                table: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_UserIdLastUpdatedBy",
                table: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropColumn(
                name: "UserIdLastUpdatedBy",
                table: "REPORTING_MY_TIME_MOVEMENTS");
        }
    }
}
