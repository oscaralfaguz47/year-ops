using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class UpdatedConsultantRolesQualityLevelsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_ROLES_QUALITY_LEVELS_UpdatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_ROLES_QUALITY_LEVELS_Users_UpdatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_ROLES_QUALITY_LEVELS_Users_UpdatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_ROLES_QUALITY_LEVELS_UpdatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");
        }
    }
}
