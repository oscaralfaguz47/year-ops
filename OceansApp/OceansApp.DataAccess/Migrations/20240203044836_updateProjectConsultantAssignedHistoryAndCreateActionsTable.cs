using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateProjectConsultantAssignedHistoryAndCreateActionsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.AlterColumn<decimal>(
                name: "OldValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(130)",
                oldMaxLength: 130,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(130)",
                oldMaxLength: 130,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NewValueDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValueDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS",
                columns: table => new
                {
                    ActionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS", x => x.ActionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "ActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "ActionId",
                principalTable: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS",
                principalColumn: "ActionId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropTable(
                name: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "NewValueDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "OldValueDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }
    }
}
