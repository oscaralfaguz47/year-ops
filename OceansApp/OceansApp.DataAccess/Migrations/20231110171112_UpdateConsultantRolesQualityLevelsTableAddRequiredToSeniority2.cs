using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class UpdateConsultantRolesQualityLevelsTableAddRequiredToSeniority2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
            name: "ConsultantSeniorityId",
            table: "CONSULTANT_ROLES_QUALITY_LEVELS",
            nullable: false,
            oldClrType: typeof(int),
            oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
