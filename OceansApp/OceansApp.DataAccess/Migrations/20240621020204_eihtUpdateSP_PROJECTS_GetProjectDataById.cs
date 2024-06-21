using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class eihtUpdateSP_PROJECTS_GetProjectDataById : Migration
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
                WHERE ProjectId = @ProjectId;
            WITH LastActions AS (
            SELECT
            H.ProjectConsultantAssignedId,
            CONVERT(VARCHAR, H.ActionDate, 120) + '; ' + AC.Name AS ActionInfo,
            H.ActionDate,
            H.Id,
            ROW_NUMBER() OVER(
                PARTITION BY H.ProjectConsultantAssignedId 
                ORDER BY H.ActionDate DESC, H.Id DESC
            ) AS RowNum
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY H
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS AC ON H.ActionId = AC.ActionId
            WHERE AC.Name IN ('Consultant Activated', 'Consultant Deactivated')
            ),
            TopAction AS (
                SELECT
                    *,
                    ROW_NUMBER() OVER(
                        PARTITION BY ProjectConsultantAssignedId
                        ORDER BY
                            CASE WHEN ActionDate >= CONVERT(date, GETDATE()) THEN 0 ELSE 1 END,
                            CASE WHEN ActionDate >= CONVERT(date, GETDATE()) THEN ActionDate ELSE NULL END ASC,
                            CASE WHEN ActionDate < CONVERT(date, GETDATE()) THEN ActionDate ELSE NULL END DESC,
                            Id DESC
                    ) AS TopRowNum
                FROM LastActions
            )
            SELECT
            CA.ProjectConsultantAssignedId,
            CA.ConsultantId,
            U.Name + ' ' + U.LastName AS ConsultantName,
            CA.HourlyClientRate,
            CA.HourlySalary,
            CA.MonthlyClientRate,
            CA.MonthlySalary,
            UC.Name AS UserCategoryName,
            TA.ActionInfo AS StatusAction
            FROM PROJECTS_CONSULTANTS_ASSIGNED CA
            JOIN CONSULTANT_DETAILS CD ON CA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            LEFT JOIN TopAction TA ON CA.ProjectConsultantAssignedId = TA.ProjectConsultantAssignedId AND TA.TopRowNum = 1
            WHERE CA.ProjectId = @ProjectId;
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
                WHERE ProjectId = @ProjectId;
            WITH LastActions AS (
            SELECT
            H.ProjectConsultantAssignedId,
            CONVERT(VARCHAR, H.ActionDate, 120) + '; ' + AC.Name AS ActionInfo,
            H.ActionDate,
            H.Id,
            ROW_NUMBER() OVER(
                PARTITION BY H.ProjectConsultantAssignedId 
                ORDER BY H.ActionDate DESC, H.Id DESC
            ) AS RowNum
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY H
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS AC ON H.ActionId = AC.ActionId
            WHERE AC.Name IN ('Consultant Activated', 'Consultant Deactivated')
            ),
            TopAction AS (
                SELECT
                    *,
                    ROW_NUMBER() OVER(
                        PARTITION BY ProjectConsultantAssignedId
                        ORDER BY
                            CASE WHEN ActionDate >= CONVERT(date, GETDATE()) THEN 0 ELSE 1 END,
                            CASE WHEN ActionDate >= CONVERT(date, GETDATE()) THEN ActionDate ELSE NULL END ASC,
                            CASE WHEN ActionDate < CONVERT(date, GETDATE()) THEN ActionDate ELSE NULL END DESC,
                            Id DESC
                    ) AS TopRowNum
                FROM LastActions
            )
            SELECT
            CA.ProjectConsultantAssignedId,
            CA.ConsultantId,
            U.Name + ' ' + U.LastName AS ConsultantName,
            CA.HourlyClientRate,
            CA.HourlySalary,
            CA.MonthlyClientRate,
            CA.MonthlySalary,
            CA.PositionDetail,
            UC.Name AS UserCategoryName,
            TA.ActionInfo AS StatusAction
            FROM PROJECTS_CONSULTANTS_ASSIGNED CA
            JOIN CONSULTANT_DETAILS CD ON CA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            LEFT JOIN TopAction TA ON CA.ProjectConsultantAssignedId = TA.ProjectConsultantAssignedId AND TA.TopRowNum = 1
            WHERE CA.ProjectId = @ProjectId;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_GetProjectDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
