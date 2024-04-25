using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class seventhUpdateSP_CONSULTANT_DETAILS_GetConsultantDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_DETAILS_GetConsultantDataById
    @ConsultantId INT
    AS
    BEGIN
   SELECT 
      CD.IdCountry
	  ,CO.Name AS CountryName
      ,CD.Phone2
      ,CD.CompanyId
	  ,CD.PaymentMethodId
	  ,PM.Name AS PaymentMethodName
      ,CD.Address
      ,CD.PersonalEmail
      ,CD.Location
      ,CD.ConsultantId
	  ,U.Name
	  ,U.LastName
	  ,U.Email
	  ,U.PhoneNumber
	  ,U.UserCategoryId
	  ,UC.Name AS UserCategoryName
	  ,R.Name AS UserRole
	  ,CD.PaymentPeriod
      ,CD.ParticipatesInOnCalls
      ,PA.PartnerId
	  ,PA.Name AS PartnerName
	  ,CH.ConsultantHolidayId
	  ,CH.Name AS ConsultantHolidayName
      ,CH.Year AS ConsultantYear
  FROM CONSULTANT_DETAILS CD
  JOIN Users U ON CD.UserId = U.Id
  JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
  JOIN UserRoles UR ON CD.UserId = UR.UserId
  JOIN Roles R ON UR.RoleId = R.Id
  JOIN COUNTRY CO ON CD.IdCountry = CO.IdCountry
  LEFT JOIN PAYMENT_METHODS PM ON CD.PaymentMethodId = PM.PaymentMethodId
  LEFT JOIN PARTNERS PA ON CD.PartnerId = PA.PartnerId
  LEFT JOIN CONSULTANT_HOLIDAYS CH ON CD.ConsultantHolidayId = CH.ConsultantHolidayId
  WHERE CD.ConsultantId = @ConsultantId

  SELECT 
  CAP.ConsultantPositionId 
  FROM CONSULTANTS_AND_POSITIONS CAP
  JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
  WHERE CAP.ConsultantId = @ConsultantId
        END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_DETAILS_GetConsultantDataById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_DETAILS_GetConsultantDataById
    @ConsultantId INT
    AS
    BEGIN
   SELECT 
      CD.IdCountry
	  ,CO.Name AS CountryName
      ,CD.Phone2
      ,CD.CompanyId
	  ,CD.PaymentMethodId
	  ,PM.Name AS PaymentMethodName
      ,CD.Address
      ,CD.PersonalEmail
      ,CD.Location
      ,CD.ConsultantId
	  ,U.Name
	  ,U.LastName
	  ,U.Email
	  ,U.PhoneNumber
	  ,U.UserCategoryId
	  ,UC.Name AS UserCategoryName
	  ,R.Name AS UserRole
	  ,CD.PaymentPeriod
      ,CD.ParticipatesInOnCalls
      ,PA.PartnerId
	  ,PA.Name AS PartnerName
	  ,CH.ConsultantHolidayId
	  ,CH.Name AS ConsultantHolidayName
  FROM CONSULTANT_DETAILS CD
  JOIN Users U ON CD.UserId = U.Id
  JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
  JOIN UserRoles UR ON CD.UserId = UR.UserId
  JOIN Roles R ON UR.RoleId = R.Id
  JOIN COUNTRY CO ON CD.IdCountry = CO.IdCountry
  LEFT JOIN PAYMENT_METHODS PM ON CD.PaymentMethodId = PM.PaymentMethodId
  LEFT JOIN PARTNERS PA ON CD.PartnerId = PA.PartnerId
  LEFT JOIN CONSULTANT_HOLIDAYS CH ON CD.ConsultantHolidayId = CH.ConsultantHolidayId
  WHERE CD.ConsultantId = @ConsultantId

  SELECT 
  CAP.ConsultantPositionId 
  FROM CONSULTANTS_AND_POSITIONS CAP
  JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
  WHERE CAP.ConsultantId = @ConsultantId
        END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_DETAILS_GetConsultantDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
