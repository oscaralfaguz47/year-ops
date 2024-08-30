using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory
    @ConsultantId INT,
    @ProjectId INT,
    @EndDate DATE
    AS
    BEGIN
         SELECT TOP 1
            PositionId
            ,MonthlySalary
            ,MonthlySalaryPartner
            ,AccessToTrackingTool
            ,HolidaysMustBePaid
            ,HourlyClientRate
            ,HourlySalary
            ,IsActive
            ,IsDefaultProject
            ,IsMonthlySalaryCalculatedPerHour
            ,MonthlyClientRate
            ,PartnerId
            ,PartnerPaysBenefits
        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
        WHERE PCA.ProjectId = @ProjectId AND PCA.ConsultantId = @ConsultantId
        AND PCAH.ActionDate <= @EndDate 
        ORDER BY  PCAH.ActionDate DESC, PCAH.Id DESC
        END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory");
        }
    }
}
