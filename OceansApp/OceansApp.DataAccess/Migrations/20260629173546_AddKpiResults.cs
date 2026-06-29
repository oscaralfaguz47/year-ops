using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddKpiResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KPI_RESULTS",
                columns: table => new
                {
                    KpiResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiDefinitionId = table.Column<int>(type: "int", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPI_RESULTS", x => x.KpiResultId);
                    table.ForeignKey(
                        name: "FK_KPI_RESULTS_KPI_DEFINITIONS_KpiDefinitionId",
                        column: x => x.KpiDefinitionId,
                        principalTable: "KPI_DEFINITIONS",
                        principalColumn: "KpiDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KPI_RESULTS_KpiDefinitionId_WeekStart",
                table: "KPI_RESULTS",
                columns: new[] { "KpiDefinitionId", "WeekStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPI_RESULTS_WeekStart",
                table: "KPI_RESULTS",
                column: "WeekStart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KPI_RESULTS");
        }
    }
}
