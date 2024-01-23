using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addConsultantAndPositionTableAndUpdateConsultantDetailsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_CONSULTANT_POSITIONS_ConsultantPositionId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantPositionId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "ConsultantPositionId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.CreateTable(
                name: "CONSULTANTS_AND_POSITIONS",
                columns: table => new
                {
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    ConsultantPositionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANTS_AND_POSITIONS", x => new { x.ConsultantId, x.ConsultantPositionId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANTS_AND_POSITIONS");

            migrationBuilder.AddColumn<int>(
                name: "ConsultantPositionId",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantPositionId",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_DETAILS_CONSULTANT_POSITIONS_ConsultantPositionId",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantPositionId",
                principalTable: "CONSULTANT_POSITIONS",
                principalColumn: "ConsultantPositionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
