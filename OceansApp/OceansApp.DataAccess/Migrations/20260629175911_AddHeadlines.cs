using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddHeadlines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HEADLINES",
                columns: table => new
                {
                    HeadlineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HEADLINES", x => x.HeadlineId);
                    table.ForeignKey(
                        name: "FK_HEADLINES_TEAMS_TeamId",
                        column: x => x.TeamId,
                        principalTable: "TEAMS",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HEADLINES_TeamId_WeekStart",
                table: "HEADLINES",
                columns: new[] { "TeamId", "WeekStart" });

            migrationBuilder.CreateIndex(
                name: "IX_HEADLINES_WeekStart",
                table: "HEADLINES",
                column: "WeekStart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HEADLINES");
        }
    }
}
