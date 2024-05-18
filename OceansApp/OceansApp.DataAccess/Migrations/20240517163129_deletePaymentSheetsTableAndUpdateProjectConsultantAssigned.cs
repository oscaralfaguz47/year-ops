using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class deletePaymentSheetsTableAndUpdateProjectConsultantAssigned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PAYMENT_SHEETS");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalaryThirdParty",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlySalaryThirdParty",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.CreateTable(
                name: "PAYMENT_SHEETS",
                columns: table => new
                {
                    PaymentSheetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantDetailCreatedByConsultantId = table.Column<int>(type: "int", nullable: false),
                    ConsultantIdCreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "date", nullable: false),
                    DateTo = table.Column<DateTime>(type: "date", nullable: false),
                    PaymentPeriod = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT_SHEETS", x => x.PaymentSheetId);
                    table.ForeignKey(
                        name: "FK_PAYMENT_SHEETS_CONSULTANT_DETAILS_ConsultantDetailCreatedByConsultantId",
                        column: x => x.ConsultantDetailCreatedByConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_SHEETS_ConsultantDetailCreatedByConsultantId",
                table: "PAYMENT_SHEETS",
                column: "ConsultantDetailCreatedByConsultantId");
        }
    }
}
