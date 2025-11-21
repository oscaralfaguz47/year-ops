using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createConsultantBenefitResetsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONSULTANT_BENEFITS_RESETS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ArrayResetValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserIdCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_BENEFITS_RESETS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_BENEFITS_RESETS_Users_UserIdCreatedBy",
                        column: x => x.UserIdCreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_RESETS_UserIdCreatedBy",
                table: "CONSULTANT_BENEFITS_RESETS",
                column: "UserIdCreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANT_BENEFITS_RESETS");
        }
    }
}
