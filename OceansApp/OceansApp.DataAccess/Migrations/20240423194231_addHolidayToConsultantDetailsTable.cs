using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addHolidayToConsultantDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultantHolidayId",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantHolidayId",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantHolidayId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_DETAILS_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantHolidayId",
                principalTable: "CONSULTANT_HOLIDAYS",
                principalColumn: "ConsultantHolidayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantHolidayId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "ConsultantHolidayId",
                table: "CONSULTANT_DETAILS");
        }
    }
}
