using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class sixthUpdateSP_PROJECTS_GetProjectDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_GetProjectDataById
            @ProjectId INT
            AS
            BEGIN
            SELECT 
                ProjectId
                ,P.Name
                ,P.Description
                ,P.StartDate
                ,P.IsActive
                ,P.IsBillable
                ,P.ClientId
                ,C.Name AS ClientName
                ,P.SuccessManagerId
                ,U.Name + ' ' + U.LastName AS SuccessManagerName
                ,P.ClientHasTrackingTool
                FROM PROJECTS P
                JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                JOIN CLIENT C ON P.ClientId = C.ClientId
                WHERE ProjectId = @ProjectId

            SELECT
                CA.ProjectConsultantAssignedId
                ,CA.ConsultantId
                ,U.Name + ' ' + U.LastName AS ConsultantName
                ,CA.HourlyClientRate
                ,CA.HourlySalary
                ,CA.MonthlyClientRate
                ,CA.MonthlySalary
                ,CA.PositionDetail
                ,CA.IsActive
                ,UC.Name AS UserCategoryName
                FROM PROJECTS_CONSULTANTS_ASSIGNED CA
                JOIN CONSULTANT_DETAILS CD ON CA.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                WHERE ProjectId = @ProjectId
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_GetProjectDataById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECTS_GetProjectDataById
            @ProjectId INT
            AS
            BEGIN
            SELECT 
                ProjectId
                ,P.Name
                ,P.Description
                ,P.StartDate
                ,P.IsActive
                ,P.IsBillable
                ,P.ClientId
                ,C.Name AS ClientName
                ,P.SuccessManagerId
                ,U.Name + ' ' + U.LastName AS SuccessManagerName
                ,P.ClientHasTrackingTool
                FROM PROJECTS P
                JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                JOIN CLIENT C ON P.ClientId = C.ClientId
                WHERE ProjectId = @ProjectId

            SELECT
                CA.ProjectConsultantAssignedId
                ,CA.ConsultantId
                ,U.Name + ' ' + U.LastName AS ConsultantName
                ,CA.HourlyClientRate
                ,CA.HourlySalary
                ,CA.MonthlyClientRate
                ,CA.MonthlySalary
                ,CA.PositionDetail
                ,CA.IsActive
                FROM PROJECTS_CONSULTANTS_ASSIGNED CA
                JOIN CONSULTANT_DETAILS CD ON CA.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                WHERE ProjectId = @ProjectId
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_GetProjectDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
