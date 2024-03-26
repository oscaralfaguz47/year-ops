using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_INTERVIEWS_GetAllInterviewsWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "INTERVIEWS",
                columns: table => new
                {
                    InterviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsultantIdCreatedBy = table.Column<int>(type: "int", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsultantIdLastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INTERVIEWS", x => x.InterviewId);
                    table.ForeignKey(
                        name: "FK_INTERVIEWS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_INTERVIEWS_CONSULTANT_DETAILS_ConsultantIdCreatedBy",
                        column: x => x.ConsultantIdCreatedBy,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_INTERVIEWS_CONSULTANT_DETAILS_ConsultantIdLastUpdatedBy",
                        column: x => x.ConsultantIdLastUpdatedBy,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_INTERVIEWS_ConsultantId",
                table: "INTERVIEWS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_INTERVIEWS_ConsultantIdCreatedBy",
                table: "INTERVIEWS",
                column: "ConsultantIdCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_INTERVIEWS_ConsultantIdLastUpdatedBy",
                table: "INTERVIEWS",
                column: "ConsultantIdLastUpdatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "INTERVIEWS");
        }
    }
}
