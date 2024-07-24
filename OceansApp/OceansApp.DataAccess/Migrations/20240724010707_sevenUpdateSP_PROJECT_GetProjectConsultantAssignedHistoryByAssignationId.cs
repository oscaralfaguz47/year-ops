using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class sevenUpdateSP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId
            @ProjectConsultantAssignedId INT,
            @UserCategoryName VARCHAR(30)
            AS
            BEGIN
            SELECT 
              PCAH.Id
              ,ActionDate
          	  ,CP.Name AS PositionName
          	  ,HourlySalary
              ,MonthlySalary
          	  ,IsMonthlySalaryCalculatedPerHour
          	  ,MonthlySalaryPartner
          	  ,PA.Name AS PartnerName
              ,PartnerPaysBenefits
          	  ,HourlyClientRate
          	  ,MonthlyClientRate
              ,AccessToTrackingTool
              ,HolidaysMustBePaid
              ,PCAH.IsActive
              ,IsDefaultProject
              ,CONCAT(U.Name,' ', U.LastName) AS UserActionedBy
          	  ,PCAH.CreationDate
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA 
            ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
            INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            INNER JOIN Users UC ON CD.UserId = UC.Id
            INNER JOIN UserCategories UCA ON UC.UserCategoryId = UCA.UserCategoryId
            INNER JOIN CONSULTANT_POSITIONS CP ON PCAH.PositionId = CP.ConsultantPositionId
            INNER JOIN Users U ON PCAH.UserIdActionedBy = U.Id
            LEFT JOIN PARTNERS PA ON PCAH.PartnerId = PA.PartnerId
            WHERE PCAH.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            AND (@UserCategoryName IS NULL OR UCA.Name = @UserCategoryName)
            ORDER BY PCAH.ActionDate DESC, PCAH.ID DESC;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId
            @ProjectConsultantAssignedId INT,
            @UserCategoryName VARCHAR(50)
            AS
            BEGIN
            SELECT 
              H.ActionDate
              ,UAB.Name + ' ' + UAB.LastName AS UserActionedBy
              ,H.NewValue
              ,H.OldValue
              ,A.Name AS Action
              ,H.NewValueDetail
              ,H.OldValueDetail
              FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY H
              JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON H.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
              JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
              JOIN Users U ON CD.UserId = U.Id
              JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
              JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS A ON H.ActionId = A.ActionId
			  JOIN CONSULTANT_DETAILS CDUAB ON H.UserActionedBy = CDUAB.ConsultantId
			  JOIN Users UAB ON CDUAB.UserId = UAB.Id
              WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
              AND (@UserCategoryName IS NULL OR UC.Name = @UserCategoryName)
              ORDER BY H.ActionDate
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
