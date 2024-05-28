using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_CONSULTANT_REIMBURSED_BENEFITS_GetReimbursementDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetReimbursementDataById
      @ReimbursedBenefitId INT
      AS
      BEGIN
      SELECT CBR.ReimbursedBenefitId
      ,CBR.BenefitId
	  ,B.Name AS BenefitName
      ,CBR.Detail
      ,CBR.ConsultantId
	  ,U.Name + ' ' + U.LastName AS ConsultantName
	  ,U.Email AS ConsultantEmail
      ,CBR.AmountReimbursed
      ,CBR.DateToBeReimbursed
      ,CBR.BenefitCategoryId
	  ,CBC.Name AS BenefitCategoryName
      FROM CONSULTANT_REIMBURSED_BENEFITS CBR
      JOIN CONSULTANT_BENEFITS B ON CBR.BenefitId = B.BenefitId
      JOIN CONSULTANT_DETAILS CD ON CBR.ConsultantId = CD.ConsultantId
      JOIN Users U ON CD.UserId = U.Id
      JOIN CONSULTANT_BENEFIT_CATEGORIES CBC ON CBR.BenefitCategoryId = CBC.BenefitCategoryId
      WHERE CBR.ReimbursedBenefitId = @ReimbursedBenefitId;
        END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_REIMBURSED_BENEFITS_GetReimbursementDataById");
        }
    }
}
