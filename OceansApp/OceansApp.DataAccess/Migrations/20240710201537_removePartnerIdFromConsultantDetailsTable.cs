using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removePartnerIdFromConsultantDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_PARTNERS_PartnerId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_PartnerId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "CONSULTANT_DETAILS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_PartnerId",
                table: "CONSULTANT_DETAILS",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_DETAILS_PARTNERS_PartnerId",
                table: "CONSULTANT_DETAILS",
                column: "PartnerId",
                principalTable: "PARTNERS",
                principalColumn: "PartnerId");
        }
    }
}
