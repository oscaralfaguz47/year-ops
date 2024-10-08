using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removeProvidersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROVIDER_EVENT_DATES");

            migrationBuilder.DropTable(
                name: "PROVIDER_EVENTS");

            migrationBuilder.DropTable(
                name: "PROVIDER");

            migrationBuilder.DropTable(
                name: "PROVIDER_CATEGORY");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROVIDER_CATEGORY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProviderCategoryCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVIDER_CATEGORY", x => x.Id);
                });

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
                name: "PROVIDER",
                columns: table => new
                {
                    ProviderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    Id = table.Column<int>(type: "int", maxLength: 8, nullable: false),
                    IdCountry = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CompanyId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ConsultantCategory = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(249)", maxLength: 249, nullable: true),
                    HourlyClientRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HourlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    IsTheMonthlyClientRateCalculatePerHour = table.Column<bool>(type: "bit", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyClientRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PersonalEmail = table.Column<string>(type: "nvarchar(249)", maxLength: 249, nullable: true),
                    Phone1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProviderCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShirtSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVIDER", x => x.ProviderId);
                    table.ForeignKey(
                        name: "FK_PROVIDER_CLIENT_ClientId",
                        column: x => x.ClientId,
                        principalTable: "CLIENT",
                        principalColumn: "ClientId");
                    table.ForeignKey(
                        name: "FK_PROVIDER_COUNTRY_IdCountry",
                        column: x => x.IdCountry,
                        principalTable: "COUNTRY",
                        principalColumn: "IdCountry",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROVIDER_PROVIDER_CATEGORY_Id",
                        column: x => x.Id,
                        principalTable: "PROVIDER_CATEGORY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROVIDER_EVENT_DATES",
                columns: table => new
                {
                    ProviderDateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderEventId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_PROVIDER_ClientId",
                table: "PROVIDER",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_Id",
                table: "PROVIDER",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_IdCountry",
                table: "PROVIDER",
                column: "IdCountry");

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
    }
}
