using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class fifthUpdateSP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant
        @ConsultantId INT,
        @BenefitId INT,
        @Year INT,
        @AmountToBeReimbursed DECIMAL(10,2),
        @ReimbursedBenefitIdToIgnore INT
        AS
        BEGIN
        SELECT 
        ISNULL(SUM(CASE WHEN TS.Name <> 'Rejected' THEN CRB.AmountReimbursed ELSE 0 END), 0) AS ConsumedAmount,
        CASE 
            WHEN ISNULL(SUM(CASE WHEN TS.Name <> 'Rejected' THEN CRB.AmountReimbursed ELSE 0 END), 0) + @AmountToBeReimbursed <= CB.Amount 
            AND @AmountToBeReimbursed <= CB.Amount THEN 1
            ELSE 0 
        END AS Applicable,
        CB.Amount AS ConfiguredBenefitAmount
        FROM 
            (SELECT 1 AS dummy) AS dummy
        LEFT JOIN 
        CONSULTANT_REIMBURSED_BENEFITS CRB ON CRB.ConsultantId = @ConsultantId
        AND CRB.BenefitId = @BenefitId
        AND YEAR(CRB.DateToBeReimbursed) = @Year
        AND (@ReimbursedBenefitIdToIgnore IS NULL OR CRB.ReimbursedBenefitId NOT IN(@ReimbursedBenefitIdToIgnore))
        LEFT JOIN 
            CONSULTANT_BENEFITS CB ON CB.BenefitId = @BenefitId
        LEFT JOIN 
        TRANSACTION_STATUSES TS ON TS.TransactionStatusId = CRB.TransactionStatusId
        GROUP BY 
        CB.Amount;
        END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant
        @ConsultantId INT,
        @BenefitId INT,
        @Year INT,
        @AmountToBeReimbursed DECIMAL(10,2),
        @ReimbursedBenefitIdToIgnore INT
        AS
        BEGIN
        SELECT 
        ISNULL(SUM(CRB.AmountReimbursed), 0) AS ConsumedAmount,
        CASE 
        WHEN ISNULL(SUM(CRB.AmountReimbursed), 0) + @AmountToBeReimbursed <= CB.Amount 
        AND @AmountToBeReimbursed <= CB.Amount THEN 1
        ELSE 0 
        END AS Applicable,
        CB.Amount AS ConfiguredBenefitAmount
        FROM 
          (SELECT 1 AS dummy) AS dummy
        LEFT JOIN 
        CONSULTANT_REIMBURSED_BENEFITS CRB ON CRB.ConsultantId = @ConsultantId
        AND CRB.BenefitId = @BenefitId
        AND YEAR(CRB.DateToBeReimbursed) = @Year
        AND (@ReimbursedBenefitIdToIgnore IS NULL OR CRB.ReimbursedBenefitId NOT IN(@ReimbursedBenefitIdToIgnore))
        JOIN 
          CONSULTANT_BENEFITS CB ON CB.BenefitId = @BenefitId
        GROUP BY 
        CB.Amount;
        END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
