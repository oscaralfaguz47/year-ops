using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class AddApplicationSystenClaimsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APPLICATION_SYSTEM_CLAIMS",
                columns: table => new
                {
                    ClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SystemSubAreaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPLICATION_SYSTEM_CLAIMS", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_APPLICATION_SYSTEM_CLAIMS_SYSTEM_SUB_AREAS_SystemSubAreaId",
                        column: x => x.SystemSubAreaId,
                        principalTable: "SYSTEM_SUB_AREAS",
                        principalColumn: "SystemSubAreaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APPLICATION_SYSTEM_CLAIMS_SystemSubAreaId",
                table: "APPLICATION_SYSTEM_CLAIMS",
                column: "SystemSubAreaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APPLICATION_SYSTEM_CLAIMS");
        }
    }
}
