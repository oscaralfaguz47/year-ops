using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant
                       @ConsultantId INT,
                       @BenefitId INT,
                       @Year INT
                       AS
                       BEGIN
                       SELECT 
                       SUM(CRB.AmountReimbursed) AS ConsumedAmount
                       FROM CONSULTANT_REIMBURSED_BENEFITS CRB
                       WHERE CRB.ConsultantId = @ConsultantId
                       AND CRB.BenefitId = @BenefitId
                       AND YEAR(DateToBeReimbursed) = @Year
                       GROUP BY CRB.ConsultantId;
                       END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant");
        }
    }
}
