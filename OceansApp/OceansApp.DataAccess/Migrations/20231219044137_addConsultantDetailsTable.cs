using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addConsultantDetailsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONSULTANT_DETAILS",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdCountry = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Phone2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalEmail = table.Column<string>(type: "nvarchar(249)", maxLength: 249, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShirtSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_DETAILS", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_DETAILS_COUNTRY_IdCountry",
                        column: x => x.IdCountry,
                        principalTable: "COUNTRY",
                        principalColumn: "IdCountry",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_DETAILS_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_IdCountry",
                table: "CONSULTANT_DETAILS",
                column: "IdCountry");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANT_DETAILS");
        }
    }
}
