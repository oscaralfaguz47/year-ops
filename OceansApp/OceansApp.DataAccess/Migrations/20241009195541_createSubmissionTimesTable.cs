using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createSubmissionTimesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    NumSentTimes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES_StartDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES_StartDate_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES",
                columns: new[] { "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES");
        }
    }
}
