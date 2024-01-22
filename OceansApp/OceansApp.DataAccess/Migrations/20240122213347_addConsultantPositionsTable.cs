using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addConsultantPositionsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultantPositionId",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CONSULTANT_POSITIONS",
                columns: table => new
                {
                    ConsultantPositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAdministrative = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_POSITIONS", x => x.ConsultantPositionId);
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_CONSULTANT_POSITIONS_ConsultantPositionId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropTable(
                name: "CONSULTANT_POSITIONS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantPositionId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "ConsultantPositionId",
                table: "CONSULTANT_DETAILS");
        }
    }
}
