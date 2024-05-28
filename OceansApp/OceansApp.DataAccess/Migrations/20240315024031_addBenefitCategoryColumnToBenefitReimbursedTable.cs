using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addBenefitCategoryColumnToBenefitReimbursedTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BenefitCategoryId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_BenefitCategoryId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "BenefitCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_CONSULTANT_BENEFIT_CATEGORIES_BenefitCategoryId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "BenefitCategoryId",
                principalTable: "CONSULTANT_BENEFIT_CATEGORIES",
                principalColumn: "BenefitCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_CONSULTANT_BENEFIT_CATEGORIES_BenefitCategoryId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_BenefitCategoryId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropColumn(
                name: "BenefitCategoryId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");
        }
    }
}
