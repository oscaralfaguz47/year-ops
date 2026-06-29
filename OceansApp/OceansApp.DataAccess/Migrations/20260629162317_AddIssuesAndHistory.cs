using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIssuesAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ISSUES",
                columns: table => new
                {
                    IssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    OriginWeekStart = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISSUES", x => x.IssueId);
                    table.ForeignKey(
                        name: "FK_ISSUES_TEAMS_TeamId",
                        column: x => x.TeamId,
                        principalTable: "TEAMS",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ISSUE_HISTORIES",
                columns: table => new
                {
                    IssueHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISSUE_HISTORIES", x => x.IssueHistoryId);
                    table.ForeignKey(
                        name: "FK_ISSUE_HISTORIES_ISSUES_IssueId",
                        column: x => x.IssueId,
                        principalTable: "ISSUES",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ISSUE_HISTORIES_IssueId_WeekStart",
                table: "ISSUE_HISTORIES",
                columns: new[] { "IssueId", "WeekStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ISSUES_OriginWeekStart",
                table: "ISSUES",
                column: "OriginWeekStart");

            migrationBuilder.CreateIndex(
                name: "IX_ISSUES_TeamId",
                table: "ISSUES",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ISSUE_HISTORIES");

            migrationBuilder.DropTable(
                name: "ISSUES");
        }
    }
}
