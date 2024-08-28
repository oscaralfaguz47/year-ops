using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateConsultantPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountingDate",
                table: "CONSULTANT_PAYMENTS",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_AccountingDate",
                table: "CONSULTANT_PAYMENTS",
                column: "AccountingDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_AccountingDate",
                table: "CONSULTANT_PAYMENTS");

            migrationBuilder.DropColumn(
                name: "AccountingDate",
                table: "CONSULTANT_PAYMENTS");
        }
    }
}
