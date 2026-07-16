using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddToDos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TODOS",
                columns: table => new
                {
                    ToDoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OriginWeekStart = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TODOS", x => x.ToDoId);
                    table.ForeignKey(
                        name: "FK_TODOS_TEAMS_TeamId",
                        column: x => x.TeamId,
                        principalTable: "TEAMS",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TODOS_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TODO_HISTORIES",
                columns: table => new
                {
                    ToDoHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToDoId = table.Column<int>(type: "int", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TODO_HISTORIES", x => x.ToDoHistoryId);
                    table.ForeignKey(
                        name: "FK_TODO_HISTORIES_TODOS_ToDoId",
                        column: x => x.ToDoId,
                        principalTable: "TODOS",
                        principalColumn: "ToDoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TODO_HISTORIES_ToDoId_WeekStart",
                table: "TODO_HISTORIES",
                columns: new[] { "ToDoId", "WeekStart" });

            migrationBuilder.CreateIndex(
                name: "IX_TODOS_OriginWeekStart",
                table: "TODOS",
                column: "OriginWeekStart");

            migrationBuilder.CreateIndex(
                name: "IX_TODOS_OwnerId",
                table: "TODOS",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TODOS_TeamId",
                table: "TODOS",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TODO_HISTORIES");

            migrationBuilder.DropTable(
                name: "TODOS");
        }
    }
}
