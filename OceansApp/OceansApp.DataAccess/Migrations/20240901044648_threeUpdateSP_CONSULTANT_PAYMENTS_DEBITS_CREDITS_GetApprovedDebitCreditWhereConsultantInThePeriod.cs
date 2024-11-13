using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class threeUpdateSP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetApprovedDebitCreditWhereConsultantInThePeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetApprovedDebitCreditWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            SELECT 
               CPDC.ConsultantPaymentDebitsCreditsId
              ,CPDC.Detail
          	  ,TT.Name AS TransactionTypeName
                ,CPDC.Amount
                ,[Quantity]
            FROM CONSULTANT_PAYMENTS_DEBITS_CREDITS CPDC
            INNER JOIN TRANSACTION_STATUSES TS ON CPDC.TransactionStatusId = TS.TransactionStatusId
            INNER JOIN TRANSACTION_TYPES TT ON CPDC.TransactionTypeId = TT.TransactionTypeId
            WHERE CPDC.ConsultantId = @ConsultantId
            AND CPDC.ActionDateWithinFortnight BETWEEN @StartDate AND @EndDate
            AND TS.Name <> 'Rejected';
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetApprovedDebitCreditWhereConsultantInThePeriod");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetApprovedDebitCreditWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            SELECT 
               CPDC.ConsultantPaymentDebitsCreditsId
              ,CPDC.Detail
          	  ,TT.Name AS TransactionTypeName
                ,CPDC.Amount
                ,[Quantity]
            FROM CONSULTANT_PAYMENTS_DEBITS_CREDITS CPDC
            INNER JOIN TRANSACTION_STATUSES TS ON CPDC.TransactionStatusId = TS.TransactionStatusId
            INNER JOIN TRANSACTION_TYPES TT ON CPDC.TransactionTypeId = TT.TransactionTypeId
            WHERE CPDC.ConsultantId = @ConsultantId
            AND CPDC.ActionDateWithinFortnight BETWEEN @StartDate AND @EndDate
            AND TS.Name = 'Approved';
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetApprovedDebitCreditWhereConsultantInThePeriod");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
