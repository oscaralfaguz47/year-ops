using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class deletePositionDetailFromProjectAssignationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_CONSULTANT_POSITIONS_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PositionId",
                principalTable: "CONSULTANT_POSITIONS",
                principalColumn: "ConsultantPositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_CONSULTANT_POSITIONS_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.AddColumn<string>(
                name: "PositionDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: false,
                defaultValue: "");
        }
    }
}
