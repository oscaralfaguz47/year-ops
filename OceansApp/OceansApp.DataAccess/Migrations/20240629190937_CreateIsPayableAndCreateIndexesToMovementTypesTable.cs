using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateIsPayableAndCreateIndexesToMovementTypesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPayable",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_IsPayable",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                column: "IsPayable");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_Name",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_IsPayable",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_Name",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES");

            migrationBuilder.DropColumn(
                name: "IsPayable",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES");
        }
    }
}
