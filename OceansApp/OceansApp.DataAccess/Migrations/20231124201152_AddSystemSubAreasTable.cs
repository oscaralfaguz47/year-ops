using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class AddSystemSubAreasTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SYSTEM_SUB_AREAS",
                columns: table => new
                {
                    SystemSubAreaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SystemAreaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSTEM_SUB_AREAS", x => x.SystemSubAreaId);
                    table.ForeignKey(
                        name: "FK_SYSTEM_SUB_AREAS_SYSTEM_AREAS_SystemAreaId",
                        column: x => x.SystemAreaId,
                        principalTable: "SYSTEM_AREAS",
                        principalColumn: "SystemAreaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SYSTEM_SUB_AREAS_SystemAreaId",
                table: "SYSTEM_SUB_AREAS",
                column: "SystemAreaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SYSTEM_SUB_AREAS");
        }
    }
}
