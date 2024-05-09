using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById
            @SubmissionId INT
            AS
            BEGIN
            SELECT
                SUB.SubmissionDate
                ,SUB.LastSubmissionDate
  	          ,P.Name AS ProjectName
  	          ,P.ClientHasTrackingTool
  	          ,CONCAT(U.Name, ' ', U.LastName) AS ConsultantName
                ,SUB.StartPeriodDate
                ,SUB.EndPeriodDate
  	          ,TimeMovements.Movements
            FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB
            INNER JOIN PROJECTS P ON SUB.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS CD ON SUB.ConsultantId = CD.ConsultantId
            INNER JOIN Users U ON CD.UserId = U.Id
             OUTER APPLY (
                        SELECT M.TimeFrom, M.TimeTo, M.Quantity, M.ActionDate, M.Notes, MT.Name AS MovementTypeName, 
  	        		  ReportsBlobs.Blobs
                        FROM REPORTING_MY_TIME_MOVEMENTS M
  	        		  INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
  	        		  OUTER APPLY (
                        SELECT B.BlobUrl
                        FROM REPORTING_MY_TIME_MOVEMENT_BLOBS B
                        WHERE B.MovementId = M.MovementId
  	        		  FOR JSON PATH
                ) AS ReportsBlobs(Blobs)
                        WHERE (M.ActionDate >= CONVERT(DATE, SUB.StartPeriodDate) AND
  	        		  M.ActionDate <= CONVERT(DATE, SUB.EndPeriodDate))
  	        		  AND M.ProjectId = P.ProjectId
  	        		  AND M.ConsultantId = CD.ConsultantId
  	        		  ORDER BY M.ActionDate 
  	        		  FOR JSON PATH
                ) AS TimeMovements(Movements)
            WHERE SUB.SubmissionId = @SubmissionId
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById");
        }
    }
}
