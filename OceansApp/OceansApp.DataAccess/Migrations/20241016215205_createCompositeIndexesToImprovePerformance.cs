using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createCompositeIndexesToImprovePerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_ConsultantId_BenefitId",
                table: "CONSULTANTS_AND_BENEFITS",
                columns: new[] { "ConsultantId", "BenefitId" });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_BenefitId_Name",
                table: "CONSULTANT_BENEFITS",
                columns: new[] { "BenefitId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_ConsultantId_BenefitId",
                table: "CONSULTANTS_AND_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_BenefitId_Name",
                table: "CONSULTANT_BENEFITS");
        }
    }
}
