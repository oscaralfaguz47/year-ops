using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addReportingMyTimeTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                columns: table => new
                {
                    MovementTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORTING_MY_TIME_MOVEMENT_TYPES", x => x.MovementTypeId);
                });

            migrationBuilder.CreateTable(
                name: "REPORTING_MY_TIME_MOVEMENTS",
                columns: table => new
                {
                    MovementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "date", nullable: false),
                    TimeFrom = table.Column<TimeSpan>(type: "time", nullable: true),
                    TimeTo = table.Column<TimeSpan>(type: "time", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false),
                    MovementTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORTING_MY_TIME_MOVEMENTS", x => x.MovementId);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENTS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENTS_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENTS_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId",
                        column: x => x.MovementTypeId,
                        principalTable: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                        principalColumn: "MovementTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENTS_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ConsultantId",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_MovementTypeId",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ProjectId",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_TransactionStatusId",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "TransactionStatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropTable(
                name: "REPORTING_MY_TIME_MOVEMENT_TYPES");
        }
    }
}
