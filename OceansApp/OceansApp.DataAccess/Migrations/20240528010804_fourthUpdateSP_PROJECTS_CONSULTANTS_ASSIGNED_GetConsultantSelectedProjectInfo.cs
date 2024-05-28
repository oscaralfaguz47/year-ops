using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fourthUpdateSP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo
            @UserId NVARCHAR(450)
            AS
            BEGIN
            SELECT 
            CD.PaymentPeriod
	        ,PUS.ProjectId
	        ,P.Name AS ProjectName
	        ,P.ClientHasTrackingTool
	        ,SMU.Name + ' ' + SMU.LastName AS SucessManagerName
	        ,SMU.Email AS SuccessManagerEmail
            ,CD.ParticipatesInOnCalls
            ,PA.PartnerId
			,PCA.AccessToTrackingTool
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
            INNER JOIN Users SMU ON SM.UserId = SMU.Id
            LEFT JOIN PARTNERS PA ON CD.PartnerId = PA.PartnerId
			INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId
			AND PCA.ConsultantId = CD.ConsultantId
            WHERE CD.UserId = @UserId;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo
            @UserId NVARCHAR(450)
            AS
            BEGIN
            SELECT 
            CD.PaymentPeriod
	        ,PUS.ProjectId
	        ,P.Name AS ProjectName
	        ,P.ClientHasTrackingTool
	        ,SMU.Name + ' ' + SMU.LastName AS SucessManagerName
	        ,SMU.Email AS SuccessManagerEmail
            ,CD.ParticipatesInOnCalls
            ,PA.PartnerId
			,PCA.AccessToTrackingTool
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
            INNER JOIN Users SMU ON SM.UserId = SMU.Id
            LEFT JOIN PARTNERS PA ON CD.PartnerId = PA.PartnerId
			INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId
			AND PCA.ConsultantId = CD.ConsultantId
            WHERE CD.UserId = @UserId
			AND PCA.AccessToTrackingTool = 1;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
