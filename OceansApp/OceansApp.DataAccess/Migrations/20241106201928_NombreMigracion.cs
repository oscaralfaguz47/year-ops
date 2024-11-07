using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NombreMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ParticipatesInOnCalls",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "ParticipatesInOnCalls",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.AddColumn<bool>(
                name: "ParticipatesInOnCalls",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ParticipatesInOnCalls",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "ParticipatesInOnCalls");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ParticipatesInOnCalls",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "ParticipatesInOnCalls",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.AddColumn<bool>(
                name: "ParticipatesInOnCalls",
                table: "CONSULTANT_DETAILS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_ParticipatesInOnCalls",
                table: "CONSULTANT_DETAILS",
                column: "ParticipatesInOnCalls");
        }
    }
}
