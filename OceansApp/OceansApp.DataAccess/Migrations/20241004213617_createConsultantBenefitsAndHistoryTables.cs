using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createConsultantBenefitsAndHistoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONSULTANTS_AND_BENEFITS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    BenefitId = table.Column<int>(type: "int", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANTS_AND_BENEFITS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CONSULTANTS_AND_BENEFITS_CONSULTANT_BENEFITS_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "CONSULTANT_BENEFITS",
                        principalColumn: "BenefitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANTS_AND_BENEFITS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTANTS_AND_BENEFITS_HISTORY",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserCreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConsultantAndBenefitId = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReimbursedBenefitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANTS_AND_BENEFITS_HISTORY", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_CONSULTANTS_AND_BENEFITS_HISTORY_CONSULTANTS_AND_BENEFITS_ConsultantAndBenefitId",
                        column: x => x.ConsultantAndBenefitId,
                        principalTable: "CONSULTANTS_AND_BENEFITS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANTS_AND_BENEFITS_HISTORY_CONSULTANT_REIMBURSED_BENEFITS_ReimbursedBenefitId",
                        column: x => x.ReimbursedBenefitId,
                        principalTable: "CONSULTANT_REIMBURSED_BENEFITS",
                        principalColumn: "ReimbursedBenefitId");
                    table.ForeignKey(
                        name: "FK_CONSULTANTS_AND_BENEFITS_HISTORY_Users_UserCreatedById",
                        column: x => x.UserCreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_BalanceAmount",
                table: "CONSULTANTS_AND_BENEFITS",
                column: "BalanceAmount");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_BenefitId",
                table: "CONSULTANTS_AND_BENEFITS",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_ConsultantId",
                table: "CONSULTANTS_AND_BENEFITS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_HISTORY_ConsultantAndBenefitId",
                table: "CONSULTANTS_AND_BENEFITS_HISTORY",
                column: "ConsultantAndBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_HISTORY_HistoryId",
                table: "CONSULTANTS_AND_BENEFITS_HISTORY",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_HISTORY_ReimbursedBenefitId",
                table: "CONSULTANTS_AND_BENEFITS_HISTORY",
                column: "ReimbursedBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANTS_AND_BENEFITS_HISTORY_UserCreatedById",
                table: "CONSULTANTS_AND_BENEFITS_HISTORY",
                column: "UserCreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANTS_AND_BENEFITS_HISTORY");

            migrationBuilder.DropTable(
                name: "CONSULTANTS_AND_BENEFITS");
        }
    }
}
