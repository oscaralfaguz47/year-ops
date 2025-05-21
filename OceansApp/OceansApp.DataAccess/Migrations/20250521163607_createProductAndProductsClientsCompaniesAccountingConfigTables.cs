using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createProductAndProductsClientsCompaniesAccountingConfigTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PRODUCTS",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Alias = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Detail = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTS", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    MovementTypeId = table.Column<int>(type: "int", nullable: true),
                    CostCenterIdSales = table.Column<int>(type: "int", nullable: false),
                    CostCenterIdSalesDiscount = table.Column<int>(type: "int", nullable: false),
                    CostCenterIdSalesReturn = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountIdSales = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountIdSalesDiscount = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountIdSalesReturn = table.Column<int>(type: "int", nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG", x => new { x.ProductId, x.ClientId, x.CompanyId });
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_ACCOUNTING_ACCOUNT_AccountingAccountIdSales",
                        column: x => x.AccountingAccountIdSales,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_ACCOUNTING_ACCOUNT_AccountingAccountIdSalesDiscount",
                        column: x => x.AccountingAccountIdSalesDiscount,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_ACCOUNTING_ACCOUNT_AccountingAccountIdSalesReturn",
                        column: x => x.AccountingAccountIdSalesReturn,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_CLIENT_ClientId",
                        column: x => x.ClientId,
                        principalTable: "CLIENT",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_COMPANIES_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANIES",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_COST_CENTER_CostCenterIdSales",
                        column: x => x.CostCenterIdSales,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_COST_CENTER_CostCenterIdSalesDiscount",
                        column: x => x.CostCenterIdSalesDiscount,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_COST_CENTER_CostCenterIdSalesReturn",
                        column: x => x.CostCenterIdSalesReturn,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_PRODUCTS_ProductId",
                        column: x => x.ProductId,
                        principalTable: "PRODUCTS",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId",
                        column: x => x.MovementTypeId,
                        principalTable: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                        principalColumn: "MovementTypeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_ProductId",
                table: "PRODUCTS",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_AccountingAccountIdSales",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "AccountingAccountIdSales");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_AccountingAccountIdSalesDiscount",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "AccountingAccountIdSalesDiscount");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_AccountingAccountIdSalesReturn",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "AccountingAccountIdSalesReturn");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_ClientId",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_CompanyId",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_CostCenterIdSales",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "CostCenterIdSales");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_CostCenterIdSalesDiscount",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "CostCenterIdSalesDiscount");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_CostCenterIdSalesReturn",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "CostCenterIdSalesReturn");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_MovementTypeId",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_ProductId",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropTable(
                name: "PRODUCTS");
        }
    }
}
