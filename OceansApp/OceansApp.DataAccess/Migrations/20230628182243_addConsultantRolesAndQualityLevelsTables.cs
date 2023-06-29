using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addConsultantRolesAndQualityLevelsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONSULTANT_QUALITY_LEVELS",
                columns: table => new
                {
                    ConsultantQualityLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_QUALITY_LEVELS", x => x.ConsultantQualityLevelId);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTANT_ROLES",
                columns: table => new
                {
                    ConsultantRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_ROLES", x => x.ConsultantRoleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANT_QUALITY_LEVELS");

            migrationBuilder.DropTable(
                name: "CONSULTANT_ROLES");
        }
    }
}
