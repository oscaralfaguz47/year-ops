using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            SELECT 
              AmountReimbursed
          	  ,CB.Name AS BenefitName
            FROM CONSULTANT_REIMBURSED_BENEFITS CRB
            INNER JOIN CONSULTANT_BENEFITS CB ON CRB.BenefitId = CB.BenefitId
            INNER JOIN TRANSACTION_STATUSES TS ON CRB.TransactionStatusId = TS.TransactionStatusId
            WHERE CRB.ConsultantId = @ConsultantId
            AND TS.Name = 'Approved'
            AND CRB.DateToBeReimbursed BETWEEN @StartDate AND @EndDate
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod");
        }
    }
}
