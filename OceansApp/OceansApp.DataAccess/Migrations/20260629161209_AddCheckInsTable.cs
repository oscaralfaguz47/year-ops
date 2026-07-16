using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHECK_INS",
                columns: table => new
                {
                    CheckInId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHECK_INS", x => x.CheckInId);
                    table.ForeignKey(
                        name: "FK_CHECK_INS_TEAMS_TeamId",
                        column: x => x.TeamId,
                        principalTable: "TEAMS",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHECK_INS_TeamId_WeekStart",
                table: "CHECK_INS",
                columns: new[] { "TeamId", "WeekStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHECK_INS_WeekStart",
                table: "CHECK_INS",
                column: "WeekStart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHECK_INS");
        }
    }
}
