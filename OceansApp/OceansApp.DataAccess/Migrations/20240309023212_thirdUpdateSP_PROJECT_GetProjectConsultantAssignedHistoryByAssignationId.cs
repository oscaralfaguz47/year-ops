using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class thirdUpdateSP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId
            @ProjectConsultantAssignedId INT
            AS
            BEGIN
            SELECT 
              H.ActionDate
              ,U.Name + ' ' + U.LastName AS UserActionedBy
              ,H.NewValue
              ,H.OldValue
              ,A.Name AS Action
              ,H.NewValueDetail
              ,H.OldValueDetail
              ,UC.Name AS UserCategory
              FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY H
              JOIN CONSULTANT_DETAILS CD ON H.UserActionedBy = CD.ConsultantId
              JOIN Users U ON CD.UserId = U.Id
              JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
              JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS A ON H.ActionId = A.ActionId
              JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON H.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
              WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
              ORDER BY H.ActionDate
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
            @ProjectConsultantAssignedId INT
            AS
            BEGIN
            SELECT 
              H.ActionDate
              ,U.Name + ' ' + U.LastName AS UserActionedBy
              ,H.NewValue
              ,H.OldValue
              ,A.Name AS Action
              ,H.NewValueDetail
              ,H.OldValueDetail
              FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY H
              JOIN CONSULTANT_DETAILS CD ON H.UserActionedBy = CD.ConsultantId
              JOIN Users U ON CD.UserId = U.Id
              JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS A ON H.ActionId = A.ActionId
              JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON H.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
              WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
              ORDER BY H.ActionDate
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
