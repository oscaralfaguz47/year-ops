using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addPartnerIdToProjectConsultantAssignedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_PARTNERS_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PartnerId",
                principalTable: "PARTNERS",
                principalColumn: "PartnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_PARTNERS_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
