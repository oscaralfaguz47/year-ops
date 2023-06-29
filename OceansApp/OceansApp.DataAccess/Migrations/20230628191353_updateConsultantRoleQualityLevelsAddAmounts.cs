using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateConsultantRoleQualityLevelsAddAmounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ClientRateMaximumAmount",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ConsultantMaximumAmount",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientRateMaximumAmount",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");

            migrationBuilder.DropColumn(
                name: "ConsultantMaximumAmount",
                table: "CONSULTANT_ROLES_QUALITY_LEVELS");
        }
    }
}
