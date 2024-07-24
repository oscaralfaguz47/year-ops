using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveConsultantsQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_POSITIONS_ConsultantId",
                table: "CONSULTANTS_AND_POSITIONS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_POSITIONS_ConsultantPositionId",
                table: "CONSULTANTS_AND_POSITIONS",
                column: "ConsultantPositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANTS_AND_POSITIONS_ConsultantId",
                table: "CONSULTANTS_AND_POSITIONS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANTS_AND_POSITIONS_ConsultantPositionId",
                table: "CONSULTANTS_AND_POSITIONS");
        }
    }
}
