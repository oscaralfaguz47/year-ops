using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateProjectsConsultantsAssignedHistoryTableAddOldValueNewValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
