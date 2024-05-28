using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createPartnersTableAndAsignToConsultants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PARTNERS",
                columns: table => new
                {
                    PartnerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactOccupation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    AdditionalEmailsForNotifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCountry = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PARTNERS", x => x.PartnerId);
                    table.ForeignKey(
                        name: "FK_PARTNERS_COUNTRY_IdCountry",
                        column: x => x.IdCountry,
                        principalTable: "COUNTRY",
                        principalColumn: "IdCountry",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_PartnerId",
                table: "CONSULTANT_DETAILS",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PARTNERS_IdCountry",
                table: "PARTNERS",
                column: "IdCountry");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_DETAILS_PARTNERS_PartnerId",
                table: "CONSULTANT_DETAILS",
                column: "PartnerId",
                principalTable: "PARTNERS",
                principalColumn: "PartnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_PARTNERS_PartnerId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropTable(
                name: "PARTNERS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_PartnerId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "CONSULTANT_DETAILS");
        }
    }
}
