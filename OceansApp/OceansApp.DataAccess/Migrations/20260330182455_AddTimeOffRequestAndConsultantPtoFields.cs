using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeOffRequestAndConsultantPtoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnualPaidTimeOffDays",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibleForPaidTimeOff",
                table: "CONSULTANT_DETAILS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TIME_OFF_REQUESTS",
                columns: table => new
                {
                    TimeOffRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    TimeOffType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    BusinessDays = table.Column<int>(type: "int", nullable: false),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserActionedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TIME_OFF_REQUESTS", x => x.TimeOffRequestId);
                    table.ForeignKey(
                        name: "FK_TIME_OFF_REQUESTS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TIME_OFF_REQUESTS_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TIME_OFF_REQUESTS_Users_UserActionedBy",
                        column: x => x.UserActionedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TIME_OFF_REQUESTS_Users_UserCreatedBy",
                        column: x => x.UserCreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TIME_OFF_REQUESTS_ConsultantId",
                table: "TIME_OFF_REQUESTS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_TIME_OFF_REQUESTS_ConsultantId_StartDate_EndDate",
                table: "TIME_OFF_REQUESTS",
                columns: new[] { "ConsultantId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TIME_OFF_REQUESTS_ConsultantId_TransactionStatusId_TimeOffType",
                table: "TIME_OFF_REQUESTS",
                columns: new[] { "ConsultantId", "TransactionStatusId", "TimeOffType" });

            migrationBuilder.CreateIndex(
                name: "IX_TIME_OFF_REQUESTS_TimeOffType",
                table: "TIME_OFF_REQUESTS",
                column: "TimeOffType");

            migrationBuilder.CreateIndex(
                name: "IX_TIME_OFF_REQUESTS_TransactionStatusId",
                table: "TIME_OFF_REQUESTS",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TIME_OFF_REQUESTS_UserActionedBy",
                table: "TIME_OFF_REQUESTS",
                column: "UserActionedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TIME_OFF_REQUESTS_UserCreatedBy",
                table: "TIME_OFF_REQUESTS",
                column: "UserCreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TIME_OFF_REQUESTS");

            migrationBuilder.DropColumn(
                name: "AnnualPaidTimeOffDays",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "IsEligibleForPaidTimeOff",
                table: "CONSULTANT_DETAILS");
        }
    }
}
