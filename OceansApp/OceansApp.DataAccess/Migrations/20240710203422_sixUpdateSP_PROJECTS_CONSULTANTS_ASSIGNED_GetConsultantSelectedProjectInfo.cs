using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class sixUpdateSP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo
            @UserId NVARCHAR(450)
            AS
            BEGIN
            WITH ConsultantProjectCount AS (
                SELECT 
                    ConsultantId,
                    COUNT(*) AS NumAssignedProjects
                FROM PROJECTS_CONSULTANTS_ASSIGNED
                GROUP BY ConsultantId
            )
            SELECT 
            CD.PaymentPeriod,
                PUS.ProjectId,
                P.Name AS ProjectName,
                P.ClientHasTrackingTool,
                SMU.Name + ' ' + SMU.LastName AS SucessManagerName,
                SMU.Email AS SuccessManagerEmail,
                CD.ParticipatesInOnCalls,
                PCA.AccessToTrackingTool,
                CPC.NumAssignedProjects
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
            INNER JOIN Users SMU ON SM.UserId = SMU.Id
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId AND PCA.ConsultantId = CD.ConsultantId
            LEFT JOIN ConsultantProjectCount CPC ON CD.ConsultantId = CPC.ConsultantId
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
            WITH ConsultantProjectCount AS (
                SELECT 
                    ConsultantId,
                    COUNT(*) AS NumAssignedProjects
                FROM PROJECTS_CONSULTANTS_ASSIGNED
                GROUP BY ConsultantId
            )
            SELECT 
            CD.PaymentPeriod,
                PUS.ProjectId,
                P.Name AS ProjectName,
                P.ClientHasTrackingTool,
                SMU.Name + ' ' + SMU.LastName AS SucessManagerName,
                SMU.Email AS SuccessManagerEmail,
                CD.ParticipatesInOnCalls,
                PA.PartnerId,
                PCA.AccessToTrackingTool,
                CPC.NumAssignedProjects
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
            INNER JOIN Users SMU ON SM.UserId = SMU.Id
            LEFT JOIN PARTNERS PA ON CD.PartnerId = PA.PartnerId
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId AND PCA.ConsultantId = CD.ConsultantId
            LEFT JOIN ConsultantProjectCount CPC ON CD.ConsultantId = CPC.ConsultantId
            WHERE CD.UserId = @UserId;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
