using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createMoreIndexesForTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_EndPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "EndPeriodDate");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_StartPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "StartPeriodDate");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ActionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_Quantity",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_ClientHasTrackingTool",
                table: "PROJECTS",
                column: "ClientHasTrackingTool");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_IsActive_ClientId_SuccessManagerId_StartDate_Name",
                table: "PROJECTS",
                columns: new[] { "IsActive", "ClientId", "SuccessManagerId", "StartDate", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_IsBillable",
                table: "PROJECTS",
                column: "IsBillable");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_Name",
                table: "PROJECTS",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_CompanyId",
                table: "LEDGER_MOVEMENT",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_Date",
                table: "LEDGER_MOVEMENT",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_LocalCredit",
                table: "LEDGER_MOVEMENT",
                column: "LocalCredit");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_LocalDebit",
                table: "LEDGER_MOVEMENT",
                column: "LocalDebit");

            migrationBuilder.CreateIndex(
                name: "IX_INTERVIEWS_Date",
                table: "INTERVIEWS",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_INTERVIEWS_DurationMinutes",
                table: "INTERVIEWS",
                column: "DurationMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_CompanyId",
                table: "DOCUMENTS_CC",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_DocumentType_DocumentDate_CompanyId_ClientId",
                table: "DOCUMENTS_CC",
                columns: new[] { "DocumentType", "DocumentDate", "CompanyId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_CompanyId",
                table: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_Status",
                table: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_AmountReimbursed",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "AmountReimbursed");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_DateToBeReimbursed",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "DateToBeReimbursed");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_Amount",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_Quantity",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_HOLIDAYS_Name",
                table: "CONSULTANT_HOLIDAYS",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_HOLIDAYS_Year",
                table: "CONSULTANT_HOLIDAYS",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_Date",
                table: "CONSULTANT_HOLIDAY_DATES",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_Name",
                table: "CONSULTANT_HOLIDAY_DATES",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_Amount",
                table: "CONSULTANT_BENEFITS",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_BenefitPeriod",
                table: "CONSULTANT_BENEFITS",
                column: "BenefitPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_EndDate",
                table: "CONSULTANT_BENEFITS",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_Name",
                table: "CONSULTANT_BENEFITS",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_StartDate",
                table: "CONSULTANT_BENEFITS",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFIT_COMPANIES_CompanyId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFIT_CATEGORIES_Name",
                table: "CONSULTANT_BENEFIT_CATEGORIES",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_AllowSentLatePaymentNotifications",
                table: "CLIENT",
                column: "AllowSentLatePaymentNotifications");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ClientCategory",
                table: "CLIENT",
                column: "ClientCategory");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ClientClass",
                table: "CLIENT",
                column: "ClientClass");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_CompanyId",
                table: "CLIENT",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_Contact",
                table: "CLIENT",
                column: "Contact");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_ContactOccupation",
                table: "CLIENT",
                column: "ContactOccupation");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_Discount",
                table: "CLIENT",
                column: "Discount");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_IsActive",
                table: "CLIENT",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_LatePaymentFee",
                table: "CLIENT",
                column: "LatePaymentFee");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_Name",
                table: "CLIENT",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_PaymentCondition",
                table: "CLIENT",
                column: "PaymentCondition");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_SuccessManager",
                table: "CLIENT",
                column: "SuccessManager");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_EndPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_StartPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ActionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_Quantity",
                table: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_ClientHasTrackingTool",
                table: "PROJECTS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_IsActive_ClientId_SuccessManagerId_StartDate_Name",
                table: "PROJECTS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_IsBillable",
                table: "PROJECTS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_Name",
                table: "PROJECTS");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_CompanyId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_Date",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_LocalCredit",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_LocalDebit",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_INTERVIEWS_Date",
                table: "INTERVIEWS");

            migrationBuilder.DropIndex(
                name: "IX_INTERVIEWS_DurationMinutes",
                table: "INTERVIEWS");

            migrationBuilder.DropIndex(
                name: "IX_DOCUMENTS_CC_CompanyId",
                table: "DOCUMENTS_CC");

            migrationBuilder.DropIndex(
                name: "IX_DOCUMENTS_CC_DocumentType_DocumentDate_CompanyId_ClientId",
                table: "DOCUMENTS_CC");

            migrationBuilder.DropIndex(
                name: "IX_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_CompanyId",
                table: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS");

            migrationBuilder.DropIndex(
                name: "IX_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_Status",
                table: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_AmountReimbursed",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_DateToBeReimbursed",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_Amount",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_Quantity",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_HOLIDAYS_Name",
                table: "CONSULTANT_HOLIDAYS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_HOLIDAYS_Year",
                table: "CONSULTANT_HOLIDAYS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_Date",
                table: "CONSULTANT_HOLIDAY_DATES");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_Name",
                table: "CONSULTANT_HOLIDAY_DATES");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_Amount",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_BenefitPeriod",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_EndDate",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_Name",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_StartDate",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFIT_COMPANIES_CompanyId",
                table: "CONSULTANT_BENEFIT_COMPANIES");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFIT_CATEGORIES_Name",
                table: "CONSULTANT_BENEFIT_CATEGORIES");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_AllowSentLatePaymentNotifications",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_ClientCategory",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_ClientClass",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_CompanyId",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_Contact",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_ContactOccupation",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_Discount",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_IsActive",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_LatePaymentFee",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_Name",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_PaymentCondition",
                table: "CLIENT");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_SuccessManager",
                table: "CLIENT");
        }
    }
}
