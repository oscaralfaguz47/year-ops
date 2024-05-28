using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetDebitCreditDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetDebitCreditDataById
            @ConsultantPaymentDebitsCreditsId INT
            AS
            BEGIN
            SELECT ConsultantPaymentDebitsCreditsId
            ,CPDC.ConsultantId
	        ,Uc.Name + ' ' + Uc.LastName AS ConsultantName
	        ,Uc.Email AS ConsultantEmail
	        ,CD.CompanyId as ConsultantCompanyId
            ,CPDC.AccountingAccountId
            ,CPDC.CostCenterId
	        ,CC.Description AS CostCenterName
            ,CPDC.Detail
	        ,CPDC.Quantity
            ,CPDC.Amount
            ,CPDC.ActionDateWithinFortnight
            ,TT.Name AS TransactionTypeName
            FROM CONSULTANT_PAYMENTS_DEBITS_CREDITS CPDC
            INNER JOIN CONSULTANT_DETAILS CD ON CPDC.ConsultantId = CD.ConsultantId
            INNER JOIN Users Uc ON CD.UserId = Uc.Id
            INNER JOIN ACCOUNTING_ACCOUNT AA ON CPDC.AccountingAccountId = AA.AccountingAccountId
            INNER JOIN COST_CENTER CC ON CPDC.CostCenterId = CC.CostCenterId
            INNER JOIN TRANSACTION_TYPES TT ON CPDC.TransactionTypeId = TT.TransactionTypeId
            WHERE CPDC.ConsultantPaymentDebitsCreditsId = @ConsultantPaymentDebitsCreditsId;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetDebitCreditDataById");
        }
    }
}
