using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSeniorityToConsultantRoleQualityLevelTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultantSeniorityId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                type: "int",
                nullable: true,
                defaultValue: null);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_ROLES_QUALITY_LEVELS_ConsultantSeniorityId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                column: "ConsultantSeniorityId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_ROLES_QUALITY_LEVELS_CONSULTANT_SENIORITIS_ConsultantSeniorityId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                column: "ConsultantSeniorityId",
                principalTable: "CONSULTANT_SENIORITIS",
                principalColumn: "ConsultantSeniorityId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_ROLES_QUALITY_LEVELS_CONSULTANT_SENIORITIS_ConsultantSeniorityId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_ROLES_QUALITY_LEVELS_ConsultantSeniorityId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.DropColumn(
                name: "ConsultantSeniorityId",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");
        }
    }
}
