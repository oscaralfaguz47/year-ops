using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twoUpdateSP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            WITH BenefitToMovementType AS (
                SELECT 'Bonusly' AS BenefitName, MT.MovementTypeId, MT.Name
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES MT
                WHERE MT.Name = 'Bonusly Rewards'
                
                UNION ALL
                
                SELECT 'Balance Program' AS BenefitName, MT.MovementTypeId, MT.Name
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES MT
                WHERE MT.Name = 'Balance Program'
                
                UNION ALL
                
                SELECT 'Oceans Challenge' AS BenefitName, MT.MovementTypeId, MT.Name
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES MT
                WHERE MT.Name = 'Oceans Challenge'
            )
            
            SELECT 
                BMT.MovementTypeId,
            	BMT.Name AS MovementTypeName,
            	    CRB.AmountReimbursed
            FROM CONSULTANT_REIMBURSED_BENEFITS CRB
            INNER JOIN CONSULTANT_BENEFITS CB ON CRB.BenefitId = CB.BenefitId
            INNER JOIN TRANSACTION_STATUSES TS ON CRB.TransactionStatusId = TS.TransactionStatusId
            LEFT JOIN BenefitToMovementType BMT ON CB.Name = BMT.BenefitName
            WHERE CRB.ConsultantId = @ConsultantId
              AND TS.Name = 'Approved'
              AND CRB.DateToBeReimbursed BETWEEN @StartDate AND @EndDate;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            WITH BenefitToMovementType AS (
                SELECT 'Bonusly' AS BenefitName, MT.MovementTypeId
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES MT
                WHERE MT.Name = 'Bonusly Rewards'
                
                UNION ALL
                
                SELECT 'Balance Program' AS BenefitName, MT.MovementTypeId
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES MT
                WHERE MT.Name = 'Balance Program'
                
                UNION ALL
                
                SELECT 'Oceans Challenge' AS BenefitName, MT.MovementTypeId
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES MT
                WHERE MT.Name = 'Oceans Challenge'
            )
            
            SELECT 
                CRB.AmountReimbursed,
                CB.Name AS BenefitName,
                BMT.MovementTypeId
            FROM CONSULTANT_REIMBURSED_BENEFITS CRB
            INNER JOIN CONSULTANT_BENEFITS CB ON CRB.BenefitId = CB.BenefitId
            INNER JOIN TRANSACTION_STATUSES TS ON CRB.TransactionStatusId = TS.TransactionStatusId
            LEFT JOIN BenefitToMovementType BMT ON CB.Name = BMT.BenefitName
            WHERE CRB.ConsultantId = @ConsultantId
              AND TS.Name = 'Approved'
              AND CRB.DateToBeReimbursed BETWEEN @StartDate AND @EndDate;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
