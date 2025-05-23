using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForProductsAndProductsClientsAccountingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ACC_ProductClientCompany",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                columns: new[] { "ProductId", "ClientId", "CompanyId" })
                .Annotation("SqlServer:Include", new[] { "TaxPercentage" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_Name_Alias_ProductCode",
                table: "PRODUCTS",
                columns: new[] { "Name", "Alias", "ProductCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ACC_ProductClientCompany",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTS_Name_Alias_ProductCode",
                table: "PRODUCTS");
        }
    }
}
