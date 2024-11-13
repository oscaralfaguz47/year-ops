using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createUsersActiveHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsersActiveHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserIdActionedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersActiveHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_UsersActiveHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersActiveHistory_Users_UserIdActionedBy",
                        column: x => x.UserIdActionedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersActiveHistory_ActionDate",
                table: "UsersActiveHistory",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_UsersActiveHistory_UserId",
                table: "UsersActiveHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersActiveHistory_UserId_IsActive_ActionDate",
                table: "UsersActiveHistory",
                columns: new[] { "UserId", "IsActive", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UsersActiveHistory_UserIdActionedBy",
                table: "UsersActiveHistory",
                column: "UserIdActionedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersActiveHistory");
        }
    }
}
