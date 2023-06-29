using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateConsultantRolesQualityLevelsTableMakePrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_ROLES_QUALITY_LEVELS_ConsultantRoleId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_ROLES_QUALITY_LEVELS",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                columns: new[] { "ConsultantRoleId", "ConsultantQualityLevelId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_ROLES_QUALITY_LEVELS",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_ROLES_QUALITY_LEVELS_ConsultantRoleId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                column: "ConsultantRoleId");
        }
    }
}
