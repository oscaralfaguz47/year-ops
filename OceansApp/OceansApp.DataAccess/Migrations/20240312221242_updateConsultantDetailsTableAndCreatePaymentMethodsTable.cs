using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateConsultantDetailsTableAndCreatePaymentMethodsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "CONSULTANT_DETAILS",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PAYMENT_METHODS",
                columns: table => new
                {
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT_METHODS", x => x.PaymentMethodId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_PaymentMethodId",
                table: "CONSULTANT_DETAILS",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_DETAILS_PAYMENT_METHODS_PaymentMethodId",
                table: "CONSULTANT_DETAILS",
                column: "PaymentMethodId",
                principalTable: "PAYMENT_METHODS",
                principalColumn: "PaymentMethodId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_PAYMENT_METHODS_PaymentMethodId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropTable(
                name: "PAYMENT_METHODS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_PaymentMethodId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "CONSULTANT_DETAILS");
        }
    }
}
