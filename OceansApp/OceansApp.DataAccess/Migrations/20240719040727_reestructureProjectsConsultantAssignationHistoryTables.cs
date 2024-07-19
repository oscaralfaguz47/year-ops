using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class reestructureProjectsConsultantAssignationHistoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_CONSULTANT_POSITIONS_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_PARTNERS_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_CONSULTANT_DETAILS_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropTable(
                name: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_NewValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionId_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectId_HourlySalary_MonthlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_IsDefaultProject",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_MonthlySalaryThirdParty",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ProjectId_ConsultantId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "NewValueDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "OldValueDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "HourlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "IsDefaultProject",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "MonthlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "MonthlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "MonthlySalaryThirdParty",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.RenameColumn(
                name: "UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "OldValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "MonthlySalaryPartner");

            migrationBuilder.RenameColumn(
                name: "NewValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "MonthlySalary");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PositionId");

            migrationBuilder.AddColumn<bool>(
                name: "AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HolidaysMustBePaid",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultProject",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PartnerPaysBenefits",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIdActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: new[] { "ProjectConsultantAssignedId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_UserIdActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "UserIdActionedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                columns: new[] { "ConsultantId", "ProjectId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_CONSULTANT_POSITIONS_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "PositionId",
                principalTable: "CONSULTANT_POSITIONS",
                principalColumn: "ConsultantPositionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PARTNERS_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "PartnerId",
                principalTable: "PARTNERS",
                principalColumn: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_Users_UserIdActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "UserIdActionedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_CONSULTANT_POSITIONS_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PARTNERS_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_Users_UserIdActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_UserIdActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "HolidaysMustBePaid",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "HourlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "IsDefaultProject",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "MonthlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "PartnerPaysBenefits",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "UserIdActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "UserActionedBy");

            migrationBuilder.RenameColumn(
                name: "MonthlySalaryPartner",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "OldValue");

            migrationBuilder.RenameColumn(
                name: "MonthlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "NewValue");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                newName: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_UserActionedBy");

            migrationBuilder.AddColumn<int>(
                name: "ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<bool>(
                name: "AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultProject",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalaryThirdParty",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "int",
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

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_NewValue",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "NewValue");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionId_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: new[] { "ProjectConsultantAssignedId", "ActionId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "AccessToTrackingTool");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectId_HourlySalary_MonthlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                columns: new[] { "ConsultantId", "ProjectId", "HourlySalary", "MonthlySalary" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_IsDefaultProject",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "IsDefaultProject");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "IsMonthlySalaryCalculatedPerHour");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_MonthlySalaryThirdParty",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "MonthlySalaryThirdParty");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ProjectId_ConsultantId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                columns: new[] { "ProjectId", "ConsultantId" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_Name",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_CONSULTANT_POSITIONS_PositionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PositionId",
                principalTable: "CONSULTANT_POSITIONS",
                principalColumn: "ConsultantPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_PARTNERS_PartnerId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "PartnerId",
                principalTable: "PARTNERS",
                principalColumn: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_CONSULTANT_DETAILS_UserActionedBy",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "UserActionedBy",
                principalTable: "CONSULTANT_DETAILS",
                principalColumn: "ConsultantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "ActionId",
                principalTable: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS",
                principalColumn: "ActionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
