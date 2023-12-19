using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateNameToConsultantHolidayDatesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantHolidayDate_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "ConsultantHolidayDate");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantHolidayDate_Users_CreatedBy",
                table: "ConsultantHolidayDate");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantHolidayDate_Users_UpdatedBy",
                table: "ConsultantHolidayDate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsultantHolidayDate",
                table: "ConsultantHolidayDate");

            migrationBuilder.RenameTable(
                name: "ConsultantHolidayDate",
                newName: "CONSULTANT_HOLIDAY_DATES");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantHolidayDate_UpdatedBy",
                table: "CONSULTANT_HOLIDAY_DATES",
                newName: "IX_CONSULTANT_HOLIDAY_DATES_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantHolidayDate_CreatedBy",
                table: "CONSULTANT_HOLIDAY_DATES",
                newName: "IX_CONSULTANT_HOLIDAY_DATES_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantHolidayDate_ConsultantHolidayId",
                table: "CONSULTANT_HOLIDAY_DATES",
                newName: "IX_CONSULTANT_HOLIDAY_DATES_ConsultantHolidayId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_HOLIDAY_DATES",
                table: "CONSULTANT_HOLIDAY_DATES",
                column: "ConsultantHolidayDateId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_HOLIDAY_DATES_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "CONSULTANT_HOLIDAY_DATES",
                column: "ConsultantHolidayId",
                principalTable: "CONSULTANT_HOLIDAYS",
                principalColumn: "ConsultantHolidayId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_HOLIDAY_DATES_Users_CreatedBy",
                table: "CONSULTANT_HOLIDAY_DATES",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_HOLIDAY_DATES_Users_UpdatedBy",
                table: "CONSULTANT_HOLIDAY_DATES",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_HOLIDAY_DATES_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "CONSULTANT_HOLIDAY_DATES");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_HOLIDAY_DATES_Users_CreatedBy",
                table: "CONSULTANT_HOLIDAY_DATES");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_HOLIDAY_DATES_Users_UpdatedBy",
                table: "CONSULTANT_HOLIDAY_DATES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_HOLIDAY_DATES",
                table: "CONSULTANT_HOLIDAY_DATES");

            migrationBuilder.RenameTable(
                name: "CONSULTANT_HOLIDAY_DATES",
                newName: "ConsultantHolidayDate");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_UpdatedBy",
                table: "ConsultantHolidayDate",
                newName: "IX_ConsultantHolidayDate_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_CreatedBy",
                table: "ConsultantHolidayDate",
                newName: "IX_ConsultantHolidayDate_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_ConsultantHolidayId",
                table: "ConsultantHolidayDate",
                newName: "IX_ConsultantHolidayDate_ConsultantHolidayId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsultantHolidayDate",
                table: "ConsultantHolidayDate",
                column: "ConsultantHolidayDateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantHolidayDate_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "ConsultantHolidayDate",
                column: "ConsultantHolidayId",
                principalTable: "CONSULTANT_HOLIDAYS",
                principalColumn: "ConsultantHolidayId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantHolidayDate_Users_CreatedBy",
                table: "ConsultantHolidayDate",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantHolidayDate_Users_UpdatedBy",
                table: "ConsultantHolidayDate",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
