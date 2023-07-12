using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addDocumentsCCTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DOCUMENTS_CC",
                columns: table => new
                {
                    DocumentCCId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ApplicationDescription = table.Column<string>(type: "nvarchar(249)", maxLength: 249, nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Canceled = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    IdSeat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCUMENTS_CC", x => x.DocumentCCId);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_CC_CLIENT_ClientId",
                        column: x => x.ClientId,
                        principalTable: "CLIENT",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_ClientId",
                table: "DOCUMENTS_CC",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DOCUMENTS_CC");
        }
    }
}
