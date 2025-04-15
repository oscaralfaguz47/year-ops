using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class sixUpdateSP_PROJECT_GetAssignedConsultantToProjectById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECT_GetAssignedConsultantToProjectById
            @ProjectConsultantAssignedId INT
            AS
            BEGIN
            SELECT TOP 1
                CONCAT(U.Name,' ', U.LastName) AS ConsultantName,
                U.Email,
            	UC.Name AS UserCategoryName,
                CD.ConsultantId,
                PCAH.ActionDate,
                PCAH.PositionId,
                PCAH.MonthlySalary,
                PCAH.HourlySalary,
                PCAH.MonthlySalaryPartner,
            	PCAH.MonthlyClientRate,
            	PCAH.HourlyClientRate,
                PCAH.CreationDate,
                PCAH.AccessToTrackingTool,
                PCAH.HolidaysMustBePaid,
                PCAH.IsDefaultProject,
                PCAH.IsMonthlySalaryCalculatedPerHour,
                PCAH.PartnerId,
                PCAH.PartnerPaysBenefits,
                PCAH.ParticipatesInOnCalls,
                PCAH.NumHoursForHoliday
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA 
            ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
            INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            INNER JOIN Users U ON CD.UserId = U.Id
            INNER JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            WHERE PCAH.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            ORDER BY
                PCAH.ActionDate DESC,
                PCAH.Id DESC;
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
            SELECT TOP 1
                CONCAT(U.Name,' ', U.LastName) AS ConsultantName,
                U.Email,
            	UC.Name AS UserCategoryName,
                CD.ConsultantId,
                PCAH.ActionDate,
                PCAH.PositionId,
                PCAH.MonthlySalary,
                PCAH.HourlySalary,
                PCAH.MonthlySalaryPartner,
            	PCAH.MonthlyClientRate,
            	PCAH.HourlyClientRate,
                PCAH.CreationDate,
                PCAH.AccessToTrackingTool,
                PCAH.HolidaysMustBePaid,
                PCAH.IsDefaultProject,
                PCAH.IsMonthlySalaryCalculatedPerHour,
                PCAH.PartnerId,
                PCAH.PartnerPaysBenefits,
                PCAH.ParticipatesInOnCalls
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA 
            ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
            INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            INNER JOIN Users U ON CD.UserId = U.Id
            INNER JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            WHERE PCAH.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            ORDER BY
                PCAH.ActionDate DESC,
                PCAH.Id DESC;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECT_GetAssignedConsultantToProjectById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
