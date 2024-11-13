using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addVoidedColumnToConsultantPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Voided",
                table: "CONSULTANT_PAYMENTS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_Voided",
                table: "CONSULTANT_PAYMENTS",
                column: "Voided");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_Voided",
                table: "CONSULTANT_PAYMENTS");

            migrationBuilder.DropColumn(
                name: "Voided",
                table: "CONSULTANT_PAYMENTS");
        }
    }
}
