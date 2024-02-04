using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
              FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY H
              JOIN CONSULTANT_DETAILS CD ON H.UserActionedBy = CD.ConsultantId
              JOIN Users U ON CD.UserId = U.Id
              JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS A ON H.ActionId = A.ActionId
              JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON H.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
              WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId");
        }
    }
}
