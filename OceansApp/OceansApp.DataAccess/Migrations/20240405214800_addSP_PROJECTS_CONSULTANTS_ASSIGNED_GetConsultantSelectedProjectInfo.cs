using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
            INNER JOIN Users SMU ON SM.UserId = SMU.Id
            WHERE CD.UserId = @UserId;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");
        }
    }
}
