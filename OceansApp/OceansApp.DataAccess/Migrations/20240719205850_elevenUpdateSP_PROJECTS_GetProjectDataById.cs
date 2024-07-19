using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class elevenUpdateSP_PROJECTS_GetProjectDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_GetProjectDataById
            @ProjectId INT,
            @CurrentDate DATETIME
            AS
            BEGIN
            SELECT 
                P.Name,
                P.Description,
                P.StartDate,
                P.IsActive,
                P.IsBillable,
                P.ClientId,
                P.SuccessManagerId,
                P.ClientHasTrackingTool,
                ISNULL(
                    (
                        SELECT 
                            JSON_QUERY(
                                (
                                    SELECT 
                                        CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                                        UC.Name AS UserCategory,
                                        PCA.ProjectConsultantAssignedId,
                                        ISNULL(PCAH.IsActive, 0) AS IsActive,
                                        PCAH.ActionDate AS BeforeOrAfterStatusActionDate
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                                    INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                                    INNER JOIN Users U ON CD.UserId = U.Id
                                    INNER JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                                    LEFT JOIN (
                                        SELECT * 
                                        FROM (
                                            SELECT 
                                                PCAH.*,
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
                                        ) AS sub
                                        WHERE rn = 1
                                    ) PCAH ON PCA.ProjectConsultantAssignedId = PCAH.ProjectConsultantAssignedId
                                    WHERE PCA.ProjectId = @ProjectId
                                    ORDER BY U.Name
                                    FOR JSON PATH
                                )
                            )
                    ), '[]'
                ) AS AssignedConsultants
            FROM PROJECTS P
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
            SELECT 
                P.Name,
                P.Description,
                P.StartDate,
                P.IsActive,
                P.IsBillable,
                P.ClientId,
                P.SuccessManagerId,
                P.ClientHasTrackingTool,
                (
                    SELECT 
                        JSON_QUERY(
                            (
                                SELECT 
                                    CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                                    UC.Name AS UserCategory,
                                    PCAH.ProjectConsultantAssignedId,
                                    PCAH.IsActive,
                                    PCAH.ActionDate AS BeforeOrAfterStatusActionDate
                                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                                INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                                INNER JOIN Users U ON CD.UserId = U.Id
                                INNER JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                                LEFT JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH ON PCA.ProjectConsultantAssignedId = PCAH.ProjectConsultantAssignedId
                                WHERE PCA.ProjectId = P.ProjectId
                                AND PCAH.ActionDate <= @CurrentDate
                                ORDER BY U.Name, PCAH.ActionDate DESC, PCAH.Id DESC
                                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                            )
                        ) AS AssignedConsultants
                    FOR JSON PATH
                ) AS AssignedConsultants
            FROM PROJECTS P
            WHERE P.ProjectId = @ProjectId;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_GetProjectDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
