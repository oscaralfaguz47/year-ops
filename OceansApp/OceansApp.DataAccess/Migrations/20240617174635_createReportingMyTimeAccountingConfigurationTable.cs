using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createReportingMyTimeAccountingConfigurationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    MovementTypeId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId",
                        column: x => x.MovementTypeId,
                        principalTable: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                        principalColumn: "MovementTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_AccountingAccountId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_CostCenterId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_MovementTypeId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION_RoleId",
                table: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REPORTING_MY_TIME_ACCOUNTING_CONFIGURATION");
        }
    }
}
