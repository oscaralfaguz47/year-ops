using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class oneUpdateSP_InsertConsultantPendingSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_InsertConsultantPendingSubmissions
    @PendingSubmissions ConsultantPendingSubmissionType READONLY
             AS
             BEGIN
                 SET NOCOUNT ON;
             
                 BEGIN TRY
                     BEGIN TRANSACTION;
             
                     INSERT INTO PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS (ConsultantId, ProjectId, StartDate, EndDate)
                     SELECT ConsultantId, ProjectId, StartDate, EndDate
                     FROM @PendingSubmissions AS ps
                     WHERE NOT EXISTS (
                         SELECT 1
                         FROM PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS AS pcps
                         WHERE pcps.ConsultantId = ps.ConsultantId
                           AND pcps.ProjectId = ps.ProjectId
                           AND pcps.StartDate = ps.StartDate
                           AND pcps.EndDate = ps.EndDate
                     );
             
                     COMMIT TRANSACTION;
                 END TRY
                 BEGIN CATCH
                     -- If there is an error other than duplicate, the exception is thrown
                     IF XACT_STATE() <> 0
                     BEGIN
                         ROLLBACK TRANSACTION;
                     END
                     DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
                     DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
                     DECLARE @ErrorState INT = ERROR_STATE();
                     RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
                 END CATCH;
             END;";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_InsertConsultantPendingSubmissions");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_InsertConsultantPendingSubmissions
            @PendingSubmissions ConsultantPendingSubmissionType READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    INSERT INTO PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS (ConsultantId, ProjectId, StartDate, EndDate)
                    SELECT ConsultantId, ProjectId, StartDate, EndDate
                    FROM @PendingSubmissions;

                    COMMIT TRANSACTION;
                END TRY
                BEGIN CATCH
                    ROLLBACK TRANSACTION;
                    THROW;
                END CATCH;
            END;";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_InsertConsultantPendingSubmissions");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
