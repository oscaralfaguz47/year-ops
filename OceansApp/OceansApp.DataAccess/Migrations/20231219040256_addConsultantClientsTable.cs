using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addConsultantClientsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONSULTANT_CLIENTS",
                columns: table => new
                {
                    ConsultantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    SuccessManager = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HourlyClientRate = table.Column<double>(type: "float", nullable: true),
                    HourlySalary = table.Column<double>(type: "float", nullable: true),
                    MonthlyClientRate = table.Column<double>(type: "float", nullable: true),
                    MontlySalary = table.Column<double>(type: "float", nullable: true),
                    IsTheMonthlyClientRateCalculatePerHour = table.Column<bool>(type: "bit", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_CLIENTS", x => new { x.ConsultantId, x.ClientId, x.SuccessManager });
                    table.ForeignKey(
                        name: "FK_CONSULTANT_CLIENTS_CLIENT_ClientId",
                        column: x => x.ClientId,
                        principalTable: "CLIENT",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_CLIENTS_Users_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_CLIENTS_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_CLIENTS_Users_SuccessManager",
                        column: x => x.SuccessManager,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_CLIENTS_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_CLIENTS_ClientId",
                table: "CONSULTANT_CLIENTS",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_CLIENTS_CreatedBy",
                table: "CONSULTANT_CLIENTS",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_CLIENTS_SuccessManager",
                table: "CONSULTANT_CLIENTS",
                column: "SuccessManager");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_CLIENTS_UpdatedBy",
                table: "CONSULTANT_CLIENTS",
                column: "UpdatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANT_CLIENTS");
        }
    }
}
