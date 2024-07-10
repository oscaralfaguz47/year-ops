using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twelveUpdateSP_PROJECT_GetAssignedConsultantToProjectById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECT_GetAssignedConsultantToProjectById
            @ProjectConsultantAssignedId INT
            AS
            BEGIN
            SELECT 
            CONCAT(U.Name,' ', U.LastName) AS ConsultantName
            ,U.Email
            ,PCA.HourlyClientRate
            ,PCA.HourlySalary
            ,PCA.MonthlyClientRate
            ,PCA.MonthlySalary
            ,CD.ConsultantId
            ,CP.ConsultantPositionId AS PositionId
            ,CP.Name AS PositionName
            ,PCA.IsMonthlySalaryCalculatedPerHour
            ,PCA.MonthlySalaryThirdParty
            ,PCA.PartnerId
            ,PA.Name AS PartnerName
            ,PCA.AccessToTrackingTool
            ,PCA.IsDefaultProject
            ,UC.Name AS UserCategoryName
            FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            LEFT JOIN CONSULTANT_POSITIONS CP ON PCA.PositionId = CP.ConsultantPositionId
            LEFT JOIN PARTNERS PA ON PCA.PartnerId = PA.PartnerId
            WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECT_GetAssignedConsultantToProjectById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECT_GetAssignedConsultantToProjectById
            @ProjectConsultantAssignedId INT
            AS
            BEGIN
            SELECT 
            CONCAT(U.Name,' ', U.LastName) AS ConsultantName
            ,U.Email
            ,PCA.HourlyClientRate
            ,PCA.HourlySalary
            ,PCA.MonthlyClientRate
            ,PCA.MonthlySalary
            ,CD.ConsultantId
            ,CP.ConsultantPositionId AS PositionId
            ,CP.Name AS PositionName
            ,PCA.IsMonthlySalaryCalculatedPerHour
            ,PCA.MonthlySalaryThirdParty
            ,PCA.PartnerId
            ,PCA.AccessToTrackingTool
            ,PCA.IsDefaultProject
            ,UC.Name AS UserCategoryName
            FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            LEFT JOIN CONSULTANT_POSITIONS CP ON PCA.PositionId = CP.ConsultantPositionId
            WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECT_GetAssignedConsultantToProjectById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
