using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createProjectConsultantPendingSubmissionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ConsultantId",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ProjectId",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_StartDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                column: "StartDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS");
        }
    }
}
