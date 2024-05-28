using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class createBenetifsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONSULTANT_BENEFITS",
                columns: table => new
                {
                    BenefitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BenefitPeriod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_BENEFITS", x => x.BenefitId);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_BENEFITS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_BENEFITS_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTANT_BENEFIT_CATEGORIES",
                columns: table => new
                {
                    BenefitCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BenefitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_BENEFIT_CATEGORIES", x => x.BenefitCategoryId);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_BENEFIT_CATEGORIES_CONSULTANT_BENEFITS_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "CONSULTANT_BENEFITS",
                        principalColumn: "BenefitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTANT_REIMBURSED_BENEFITS",
                columns: table => new
                {
                    ReimbursedBenefitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BenefitId = table.Column<int>(type: "int", nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    AmountReimbursed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateToBeReimbursed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BenefitPaid = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsultantIdCreatedBy = table.Column<int>(type: "int", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsultantIdLastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_REIMBURSED_BENEFITS", x => x.ReimbursedBenefitId);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_REIMBURSED_BENEFITS_CONSULTANT_BENEFITS_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "CONSULTANT_BENEFITS",
                        principalColumn: "BenefitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_REIMBURSED_BENEFITS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_REIMBURSED_BENEFITS_CONSULTANT_DETAILS_ConsultantIdCreatedBy",
                        column: x => x.ConsultantIdCreatedBy,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_REIMBURSED_BENEFITS_CONSULTANT_DETAILS_ConsultantIdLastUpdatedBy",
                        column: x => x.ConsultantIdLastUpdatedBy,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFIT_CATEGORIES_BenefitId",
                table: "CONSULTANT_BENEFIT_CATEGORIES",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_AccountingAccountId",
                table: "CONSULTANT_BENEFITS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_CostCenterId",
                table: "CONSULTANT_BENEFITS",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_BenefitId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_ConsultantId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_ConsultantIdCreatedBy",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "ConsultantIdCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_ConsultantIdLastUpdatedBy",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "ConsultantIdLastUpdatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANT_BENEFIT_CATEGORIES");

            migrationBuilder.DropTable(
                name: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropTable(
                name: "CONSULTANT_BENEFITS");
        }
    }
}
