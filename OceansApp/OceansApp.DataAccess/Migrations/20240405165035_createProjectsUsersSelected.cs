using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class createProjectsUsersSelected : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROJECTS_USERS_SELECTED",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECTS_USERS_SELECTED", x => new { x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PROJECTS_USERS_SELECTED_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PROJECTS_USERS_SELECTED_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_USERS_SELECTED_UserId",
                table: "PROJECTS_USERS_SELECTED",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROJECTS_USERS_SELECTED");
        }
    }
}
