using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addIdPrimaryKeyToProjectConsultantAssignedHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");
        }
    }
}
