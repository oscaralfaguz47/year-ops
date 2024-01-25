using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addProjectConsultantAssignedTableAndHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROJECTS_CONSULTANTS_ASSIGNED",
                columns: table => new
                {
                    ProjectConsultantAssignedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECTS_CONSULTANTS_ASSIGNED", x => x.ProjectConsultantAssignedId);
                    table.ForeignKey(
                        name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: table => new
                {
                    ProjectConsultantAssignedId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserActionedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PROJECTS_CONSULTANTS_ASSIGNED_ProjectConsultantAssignedId",
                        column: x => x.ProjectConsultantAssignedId,
                        principalTable: "PROJECTS_CONSULTANTS_ASSIGNED",
                        principalColumn: "ProjectConsultantAssignedId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_Users_UserActionedBy",
                        column: x => x.UserActionedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ProjectId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "ProjectConsultantAssignedId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "UserActionedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropTable(
                name: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
