using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addReportingMyTimeCommentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REPORTING_MY_TIME_COMMENTS",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "date", nullable: false),
                    SubmissionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORTING_MY_TIME_COMMENTS", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_COMMENTS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_COMMENTS_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_COMMENTS_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_COMMENTS_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_COMMENTS_ConsultantId",
                table: "REPORTING_MY_TIME_COMMENTS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_COMMENTS_ProjectId",
                table: "REPORTING_MY_TIME_COMMENTS",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_COMMENTS_SubmissionId",
                table: "REPORTING_MY_TIME_COMMENTS",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_COMMENTS_UserId",
                table: "REPORTING_MY_TIME_COMMENTS",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REPORTING_MY_TIME_COMMENTS");
        }
    }
}
