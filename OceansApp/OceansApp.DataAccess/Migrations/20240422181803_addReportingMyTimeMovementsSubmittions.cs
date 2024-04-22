using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addReportingMyTimeMovementsSubmittions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    StartPeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndPeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_ConsultantId",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_ProjectId",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_TransactionStatusId",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "TransactionStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");
        }
    }
}
