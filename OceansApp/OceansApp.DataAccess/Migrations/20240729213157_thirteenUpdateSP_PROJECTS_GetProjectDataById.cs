using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class thirteenUpdateSP_PROJECTS_GetProjectDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_GetProjectDataById
            @ProjectId INT,
            @CurrentDate DATETIME
            AS
            BEGIN
            -- CTE for current history (history up to the current date)
            WITH CurrentHistory AS (
                SELECT 
                    PCAH.ProjectConsultantAssignedId, 
                    PCAH.IsActive, 
                    PCAH.ActionDate,
                    ROW_NUMBER() OVER (PARTITION BY PCAH.ProjectConsultantAssignedId 
                                       ORDER BY 
                                           CASE 
                                               WHEN PCAH.ActionDate <= @CurrentDate THEN 0 
                                               ELSE 1 
                                           END, 
                                           ABS(DATEDIFF(day, PCAH.ActionDate, @CurrentDate)), 
                                           PCAH.ActionDate DESC, 
                                           PCAH.Id DESC) AS rn
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
            ),
            -- CTE for future history (history after the current date)
            FutureHistory AS (
                SELECT 
                    PCAH.ProjectConsultantAssignedId, 
                    PCAH.IsActive AS FutureStatus, 
                    PCAH.ActionDate AS FutureStatusDate,
                    ROW_NUMBER() OVER (PARTITION BY PCAH.ProjectConsultantAssignedId 
                                       ORDER BY 
                                           PCAH.ActionDate ASC, 
                                           PCAH.Id DESC) AS rn
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                WHERE PCAH.ActionDate > @CurrentDate
            )
            
            SELECT 
                P.Name,
                P.Description,
                P.StartDate,
                P.IsActive,
                P.IsBillable,
                P.ClientId,
                CL.Name AS ClientName,
                P.SuccessManagerId,
                P.ClientHasTrackingTool,
                ISNULL((
                    SELECT JSON_QUERY((
                        SELECT 
                            CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                            UC.Name AS UserCategory,
                            PCA.ProjectConsultantAssignedId,
                            ISNULL(CH.IsActive, 0) AS IsActive,
                            CH.ActionDate AS BeforeOrAfterStatusActionDate,
                            COALESCE(FH.FutureStatus, NULL) AS FutureStatus,
                            COALESCE(FH.FutureStatusDate, NULL) AS FutureStatusDate
                        FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                        INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                        INNER JOIN Users U ON CD.UserId = U.Id
                        INNER JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                        LEFT JOIN CurrentHistory CH ON PCA.ProjectConsultantAssignedId = CH.ProjectConsultantAssignedId AND CH.rn = 1
                        LEFT JOIN FutureHistory FH ON PCA.ProjectConsultantAssignedId = FH.ProjectConsultantAssignedId AND FH.rn = 1
                        WHERE PCA.ProjectId = @ProjectId
                        ORDER BY U.Name
                        FOR JSON PATH
                    ))
                ), '[]') AS AssignedConsultants
            FROM PROJECTS P
            INNER JOIN CLIENT CL ON P.ClientId = CL.ClientId
            WHERE P.ProjectId = @ProjectId;
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
            @ProjectId INT,
            @CurrentDate DATETIME
            AS
            BEGIN
            -- CTE for current history (history up to the current date)
            WITH CurrentHistory AS (
                SELECT 
                    PCAH.ProjectConsultantAssignedId, 
                    PCAH.IsActive, 
                    PCAH.ActionDate,
                    ROW_NUMBER() OVER (PARTITION BY PCAH.ProjectConsultantAssignedId 
                                       ORDER BY 
                                           CASE 
                                               WHEN PCAH.ActionDate <= @CurrentDate THEN 0 
                                               ELSE 1 
                                           END, 
                                           ABS(DATEDIFF(day, PCAH.ActionDate, @CurrentDate)), 
                                           PCAH.ActionDate DESC, 
                                           PCAH.Id DESC) AS rn
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
            ),
            -- CTE for future history (history after the current date)
            FutureHistory AS (
                SELECT 
                    PCAH.ProjectConsultantAssignedId, 
                    PCAH.IsActive AS FutureStatus, 
                    PCAH.ActionDate AS FutureStatusDate,
                    ROW_NUMBER() OVER (PARTITION BY PCAH.ProjectConsultantAssignedId 
                                       ORDER BY 
                                           PCAH.ActionDate ASC, 
                                           PCAH.Id DESC) AS rn
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                WHERE PCAH.ActionDate > @CurrentDate
            )
            
            SELECT 
                P.Name,
                P.Description,
                P.StartDate,
                P.IsActive,
                P.IsBillable,
                P.ClientId,
                P.SuccessManagerId,
                P.ClientHasTrackingTool,
                ISNULL((
                    SELECT JSON_QUERY((
                        SELECT 
                            CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                            UC.Name AS UserCategory,
                            PCA.ProjectConsultantAssignedId,
                            ISNULL(CH.IsActive, 0) AS IsActive,
                            CH.ActionDate AS BeforeOrAfterStatusActionDate,
                            COALESCE(FH.FutureStatus, NULL) AS FutureStatus,
                            COALESCE(FH.FutureStatusDate, NULL) AS FutureStatusDate
                        FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                        INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                        INNER JOIN Users U ON CD.UserId = U.Id
                        INNER JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                        LEFT JOIN CurrentHistory CH ON PCA.ProjectConsultantAssignedId = CH.ProjectConsultantAssignedId AND CH.rn = 1
                        LEFT JOIN FutureHistory FH ON PCA.ProjectConsultantAssignedId = FH.ProjectConsultantAssignedId AND FH.rn = 1
                        WHERE PCA.ProjectId = @ProjectId
                        ORDER BY U.Name
                        FOR JSON PATH
                    ))
                ), '[]') AS AssignedConsultants
            FROM PROJECTS P
            WHERE P.ProjectId = @ProjectId;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_GetProjectDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
