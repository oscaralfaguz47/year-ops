using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class UpdateCompanyIdToMayorAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_COST_CENTER_IdCostCenter",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_LEDGER_MOVEMENT_COST_CENTER_IdCostCenter",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_IdCostCenter",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COST_CENTER",
                table: "COST_CENTER");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.DropColumn(
                name: "IdCostCenter",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "IdCostCenter",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.RenameColumn(
                name: "IdCostCenter",
                table: "COST_CENTER",
                newName: "CostCenterCode");

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "LEDGER_MOVEMENT",
                type: "int",
                maxLength: 25,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "COST_CENTER",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "COST_CENTER",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterIncreaseId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                type: "int",
                maxLength: 25,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_COST_CENTER",
                table: "COST_CENTER",
                column: "CostCenterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                column: "CostCenterIncreaseId");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_CostCenterId",
                table: "LEDGER_MOVEMENT",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_CostCenterId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                column: "CostCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_COST_CENTER_CostCenterId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                column: "CostCenterId",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LEDGER_MOVEMENT_COST_CENTER_CostCenterId",
                table: "LEDGER_MOVEMENT",
                column: "CostCenterId",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_COST_CENTER_CostCenterId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_LEDGER_MOVEMENT_COST_CENTER_CostCenterId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_CostCenterId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COST_CENTER",
                table: "COST_CENTER");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.DropIndex(
                name: "IX_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_CostCenterId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "COST_CENTER");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "COST_CENTER");

            migrationBuilder.DropColumn(
                name: "CostCenterIncreaseId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.RenameColumn(
                name: "CostCenterCode",
                table: "COST_CENTER",
                newName: "IdCostCenter");

            migrationBuilder.AddColumn<string>(
                name: "IdCostCenter",
                table: "LEDGER_MOVEMENT",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdCostCenter",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COST_CENTER",
                table: "COST_CENTER",
                column: "IdCostCenter");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                column: "IdCostCenter");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_IdCostCenter",
                table: "LEDGER_MOVEMENT",
                column: "IdCostCenter");

            migrationBuilder.AddForeignKey(
                name: "FK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_COST_CENTER_IdCostCenter",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                column: "IdCostCenter",
                principalTable: "COST_CENTER",
                principalColumn: "IdCostCenter",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LEDGER_MOVEMENT_COST_CENTER_IdCostCenter",
                table: "LEDGER_MOVEMENT",
                column: "IdCostCenter",
                principalTable: "COST_CENTER",
                principalColumn: "IdCostCenter",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
