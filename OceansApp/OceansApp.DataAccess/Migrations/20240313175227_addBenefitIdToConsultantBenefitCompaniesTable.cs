using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addBenefitIdToConsultantBenefitCompaniesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BenefitId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFIT_COMPANIES_BenefitId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                column: "BenefitId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_BENEFIT_COMPANIES_CONSULTANT_BENEFITS_BenefitId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                column: "BenefitId",
                principalTable: "CONSULTANT_BENEFITS",
                principalColumn: "BenefitId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_BENEFIT_COMPANIES_CONSULTANT_BENEFITS_BenefitId",
                table: "CONSULTANT_BENEFIT_COMPANIES");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFIT_COMPANIES_BenefitId",
                table: "CONSULTANT_BENEFIT_COMPANIES");

            migrationBuilder.DropColumn(
                name: "BenefitId",
                table: "CONSULTANT_BENEFIT_COMPANIES");
        }
    }
}
