using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addProviderEventAndProviderEventDatesTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROVIDER_EVENTS",
                columns: table => new
                {
                    ProviderEventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVIDER_EVENTS", x => x.ProviderEventId);
                });

            migrationBuilder.CreateTable(
                name: "PROVIDER_EVENT_DATES",
                columns: table => new
                {
                    ProviderDateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderEventId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVIDER_EVENT_DATES", x => x.ProviderDateId);
                    table.ForeignKey(
                        name: "FK_PROVIDER_EVENT_DATES_PROVIDER_EVENTS_ProviderEventId",
                        column: x => x.ProviderEventId,
                        principalTable: "PROVIDER_EVENTS",
                        principalColumn: "ProviderEventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROVIDER_EVENT_DATES_PROVIDER_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "PROVIDER",
                        principalColumn: "ProviderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROVIDER_EVENT_DATES_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_EVENT_DATES_CreatedBy",
                table: "PROVIDER_EVENT_DATES",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_EVENT_DATES_ProviderEventId",
                table: "PROVIDER_EVENT_DATES",
                column: "ProviderEventId");

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_EVENT_DATES_ProviderId",
                table: "PROVIDER_EVENT_DATES",
                column: "ProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROVIDER_EVENT_DATES");

            migrationBuilder.DropTable(
                name: "PROVIDER_EVENTS");
        }
    }
}
