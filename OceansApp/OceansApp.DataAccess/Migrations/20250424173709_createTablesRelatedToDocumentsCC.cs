using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createTablesRelatedToDocumentsCC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "DOCUMENTS_CC",
                type: "varchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<int>(
                name: "DocumentCCSubtypeId",
                table: "DOCUMENTS_CC",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CLIENT_ACCOUNT_CATEGORIES",
                columns: table => new
                {
                    ClientAccountingCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CostCenterIdSalesReturn = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountIdSalesReturn = table.Column<int>(type: "int", nullable: false),
                    CostCenterIdSalesDiscounts = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountIdSalesDiscounts = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    CostCenterSalesReturnCostCenterId = table.Column<int>(type: "int", nullable: false),
                    CostCenterSalesDiscount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENT_ACCOUNT_CATEGORIES", x => x.ClientAccountingCategoryId);
                    table.ForeignKey(
                        name: "FK_CLIENT_ACCOUNT_CATEGORIES_ACCOUNTING_ACCOUNT_AccountingAccountIdSalesDiscounts",
                        column: x => x.AccountingAccountIdSalesDiscounts,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLIENT_ACCOUNT_CATEGORIES_ACCOUNTING_ACCOUNT_AccountingAccountIdSalesReturn",
                        column: x => x.AccountingAccountIdSalesReturn,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLIENT_ACCOUNT_CATEGORIES_COMPANIES_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANIES",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CLIENT_ACCOUNT_CATEGORIES_COST_CENTER_CostCenterSalesReturnCostCenterId",
                        column: x => x.CostCenterSalesReturnCostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DOCUMENTS_TYPES",
                columns: table => new
                {
                    DocumentTypeId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCUMENTS_TYPES", x => x.DocumentTypeId);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_TYPES_TRANSACTION_TYPES_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TRANSACTION_TYPES",
                        principalColumn: "TransactionTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DOCUMENTS_CC_SUBTYPES",
                columns: table => new
                {
                    DocumentCCSybtypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCUMENTS_CC_SUBTYPES", x => x.DocumentCCSybtypeId);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_CC_SUBTYPES_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_CC_SUBTYPES_COMPANIES_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANIES",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_CC_SUBTYPES_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_CC_SUBTYPES_DOCUMENTS_TYPES_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DOCUMENTS_TYPES",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_DocumentCCSubtypeId",
                table: "DOCUMENTS_CC",
                column: "DocumentCCSubtypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_DocumentType",
                table: "DOCUMENTS_CC",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ACCOUNT_CATEGORIES_AccountingAccountIdSalesDiscounts",
                table: "CLIENT_ACCOUNT_CATEGORIES",
                column: "AccountingAccountIdSalesDiscounts");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ACCOUNT_CATEGORIES_AccountingAccountIdSalesReturn",
                table: "CLIENT_ACCOUNT_CATEGORIES",
                column: "AccountingAccountIdSalesReturn");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ACCOUNT_CATEGORIES_CompanyId",
                table: "CLIENT_ACCOUNT_CATEGORIES",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ACCOUNT_CATEGORIES_CostCenterIdSalesDiscounts",
                table: "CLIENT_ACCOUNT_CATEGORIES",
                column: "CostCenterIdSalesDiscounts");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ACCOUNT_CATEGORIES_CostCenterIdSalesReturn",
                table: "CLIENT_ACCOUNT_CATEGORIES",
                column: "CostCenterIdSalesReturn");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ACCOUNT_CATEGORIES_CostCenterSalesReturnCostCenterId",
                table: "CLIENT_ACCOUNT_CATEGORIES",
                column: "CostCenterSalesReturnCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_SUBTYPES_AccountingAccountId",
                table: "DOCUMENTS_CC_SUBTYPES",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_SUBTYPES_CompanyId",
                table: "DOCUMENTS_CC_SUBTYPES",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_SUBTYPES_CostCenterId",
                table: "DOCUMENTS_CC_SUBTYPES",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_SUBTYPES_DocumentTypeId",
                table: "DOCUMENTS_CC_SUBTYPES",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_TYPES_TransactionTypeId",
                table: "DOCUMENTS_TYPES",
                column: "TransactionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DOCUMENTS_CC_COMPANIES_CompanyId",
                table: "DOCUMENTS_CC",
                column: "CompanyId",
                principalTable: "COMPANIES",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DOCUMENTS_CC_DOCUMENTS_CC_SUBTYPES_DocumentCCSubtypeId",
                table: "DOCUMENTS_CC",
                column: "DocumentCCSubtypeId",
                principalTable: "DOCUMENTS_CC_SUBTYPES",
                principalColumn: "DocumentCCSybtypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DOCUMENTS_CC_COMPANIES_CompanyId",
                table: "DOCUMENTS_CC");

            migrationBuilder.DropForeignKey(
                name: "FK_DOCUMENTS_CC_DOCUMENTS_CC_SUBTYPES_DocumentCCSubtypeId",
                table: "DOCUMENTS_CC");

            migrationBuilder.DropTable(
                name: "CLIENT_ACCOUNT_CATEGORIES");

            migrationBuilder.DropTable(
                name: "DOCUMENTS_CC_SUBTYPES");

            migrationBuilder.DropTable(
                name: "DOCUMENTS_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_DOCUMENTS_CC_DocumentCCSubtypeId",
                table: "DOCUMENTS_CC");

            migrationBuilder.DropIndex(
                name: "IX_DOCUMENTS_CC_DocumentType",
                table: "DOCUMENTS_CC");

            migrationBuilder.DropColumn(
                name: "DocumentCCSubtypeId",
                table: "DOCUMENTS_CC");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "DOCUMENTS_CC",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(8)",
                oldMaxLength: 8);
        }
    }
}
