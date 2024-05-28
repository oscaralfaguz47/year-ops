using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CONSULTANT_DETAILS",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeactivationDate",
                table: "Users",
                column: "DeactivationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailConfirmed",
                table: "Users",
                column: "EmailConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id_Name_LastName",
                table: "Users",
                columns: new[] { "Id", "Name", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LockoutEnabled",
                table: "Users",
                column: "LockoutEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LockoutEnd",
                table: "Users",
                column: "LockoutEnd");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Occupation",
                table: "Users",
                column: "Occupation");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TwoFactorEnabled",
                table: "Users",
                column: "TwoFactorEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_TransactionStatusId_StartPeriodDate_EndPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                columns: new[] { "TransactionStatusId", "StartPeriodDate", "EndPeriodDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_Name",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS",
                column: "Name");

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
                name: "IX_CONSULTANT_DETAILS_Address",
                table: "CONSULTANT_DETAILS",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_CompanyId",
                table: "CONSULTANT_DETAILS",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_ParticipatesInOnCalls",
                table: "CONSULTANT_DETAILS",
                column: "ParticipatesInOnCalls");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_PaymentPeriod",
                table: "CONSULTANT_DETAILS",
                column: "PaymentPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_PersonalEmail",
                table: "CONSULTANT_DETAILS",
                column: "PersonalEmail");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_Phone2",
                table: "CONSULTANT_DETAILS",
                column: "Phone2");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_UserId_ConsultantId",
                table: "CONSULTANT_DETAILS",
                columns: new[] { "UserId", "ConsultantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DeactivationDate",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmailConfirmed",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Id_Name_LastName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LockoutEnabled",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LockoutEnd",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Occupation",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TwoFactorEnabled",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_TransactionStatusId_StartPeriodDate_EndPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_Name",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS");

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
                name: "IX_CONSULTANT_DETAILS_Address",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_CompanyId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ParticipatesInOnCalls",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_PaymentPeriod",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_PersonalEmail",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_Phone2",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_UserId_ConsultantId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CONSULTANT_DETAILS",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
