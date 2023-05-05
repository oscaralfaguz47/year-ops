using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class AddProviderAndCountryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "COUNTRY",
                columns: table => new
                {
                    IdCountry = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COUNTRY", x => x.IdCountry);
                });

            migrationBuilder.CreateTable(
                name: "PROVIDER",
                columns: table => new
                {
                    IdProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(249)", maxLength: 249, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Phone1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdCountry = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    IdProviderCategory = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVIDER", x => x.IdProvider);
                    table.ForeignKey(
                        name: "FK_PROVIDER_COUNTRY_IdCountry",
                        column: x => x.IdCountry,
                        principalTable: "COUNTRY",
                        principalColumn: "IdCountry",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROVIDER_PROVIDER_CATEGORY_IdProviderCategory",
                        column: x => x.IdProviderCategory,
                        principalTable: "PROVIDER_CATEGORY",
                        principalColumn: "IdProviderCategory",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_IdCountry",
                table: "PROVIDER",
                column: "IdCountry");

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_IdProviderCategory",
                table: "PROVIDER",
                column: "IdProviderCategory");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROVIDER");

            migrationBuilder.DropTable(
                name: "COUNTRY");
        }
    }
}
