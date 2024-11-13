using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_PAYMENT_SHEETS_GetProjectsInfoWhereConsultantIsActiveInPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetProjectsInfoWhereConsultantIsActiveInPeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            -- CTE to determine the IsActive status of projects
            WITH ActiveProjects AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    PCA.ProjectId,
                    PCA.ConsultantId
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                INNER JOIN (
                    SELECT
                        PCAH.ProjectConsultantAssignedId,
                        MAX(PCAH.ActionDate) AS MaxActionDate
                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    WHERE PCAH.ActionDate <= @EndDate
                    GROUP BY PCAH.ProjectConsultantAssignedId
                ) AS LastHistory
                ON PCA.ProjectConsultantAssignedId = LastHistory.ProjectConsultantAssignedId
                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                ON LastHistory.ProjectConsultantAssignedId = PCAH.ProjectConsultantAssignedId AND LastHistory.MaxActionDate = PCAH.ActionDate
                WHERE PCAH.IsActive = 1
            ),
            -- CTE to determine projects for which there is no record in PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS
            EnabledProjects AS (
                SELECT 
                    PCA.ProjectConsultantAssignedId,
                    PCA.ProjectId, 
                    PCA.ConsultantId
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                    WHERE PCA.ProjectId = PCPDT.ProjectId
                    AND PCA.ConsultantId = PCPDT.ConsultantId
                    AND PCPDT.StartPeriodDate <= @EndDate
                    AND PCPDT.EndPeriodDate >= @StartDate
                )
            ),
            -- CTE to get the latest history of PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
            RecentProjectHistory AS (
                SELECT
                    PCAH.ProjectConsultantAssignedId,
                    PCAH.MonthlySalary,
                    PCAH.MonthlySalaryPartner,
                    PCAH.HolidaysMustBePaid,
                    PCAH.IsDefaultProject,
                    PCAH.IsMonthlySalaryCalculatedPerHour,
                    PCAH.PartnerId,
                    PCAH.PartnerPaysBenefits,
                    ROW_NUMBER() OVER (PARTITION BY PCAH.ProjectConsultantAssignedId ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS RowNum
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                WHERE PCAH.ActionDate <= @EndDate
            )
            SELECT
                P.ProjectId,
                P.Name AS ProjectName,
                RPH.MonthlySalary,
                RPH.MonthlySalaryPartner,
                RPH.HolidaysMustBePaid,
                RPH.IsDefaultProject,
                RPH.IsMonthlySalaryCalculatedPerHour,
                RPH.PartnerId,
                RPH.PartnerPaysBenefits
            FROM (
                SELECT ProjectConsultantAssignedId, ProjectId, ConsultantId FROM ActiveProjects
                UNION
                SELECT ProjectConsultantAssignedId, ProjectId, ConsultantId FROM EnabledProjects
            ) AS AllProjects
            INNER JOIN PROJECTS P ON AllProjects.ProjectId = P.ProjectId
            INNER JOIN RecentProjectHistory RPH ON AllProjects.ProjectConsultantAssignedId = RPH.ProjectConsultantAssignedId
            WHERE RPH.RowNum = 1
            AND AllProjects.ConsultantId = @ConsultantId
            ORDER BY P.Name;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_SHEETS_GetProjectsInfoWhereConsultantIsActiveInPeriod");
        }
    }
}
