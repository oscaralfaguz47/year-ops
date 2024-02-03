using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateProjectConsultantAssignedHistoryUserActinedByAndActionDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_Users_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.RenameColumn(
                name: "AssignedDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                newName: "CreationDate");

            migrationBuilder.AlterColumn<int>(
                name: "UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_CONSULTANT_DETAILS_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "UserActionedBy",
                principalTable: "CONSULTANT_DETAILS",
                principalColumn: "ConsultantId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_CONSULTANT_DETAILS_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                newName: "AssignedDate");

            migrationBuilder.AlterColumn<string>(
                name: "UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_Users_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "UserActionedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
