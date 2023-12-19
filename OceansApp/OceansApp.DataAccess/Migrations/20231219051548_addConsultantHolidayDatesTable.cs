using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addConsultantHolidayDatesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultantHolidayDate",
                columns: table => new
                {
                    ConsultantHolidayDateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantHolidayId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultantHolidayDate", x => x.ConsultantHolidayDateId);
                    table.ForeignKey(
                        name: "FK_ConsultantHolidayDate_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                        column: x => x.ConsultantHolidayId,
                        principalTable: "CONSULTANT_HOLIDAYS",
                        principalColumn: "ConsultantHolidayId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultantHolidayDate_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultantHolidayDate_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantHolidayDate_ConsultantHolidayId",
                table: "ConsultantHolidayDate",
                column: "ConsultantHolidayId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantHolidayDate_CreatedBy",
                table: "ConsultantHolidayDate",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantHolidayDate_UpdatedBy",
                table: "ConsultantHolidayDate",
                column: "UpdatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultantHolidayDate");
        }
    }
}
