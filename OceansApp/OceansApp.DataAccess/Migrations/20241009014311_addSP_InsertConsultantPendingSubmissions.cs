using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_InsertConsultantPendingSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE TYPE ConsultantPendingSubmissionType AS TABLE
            (
                ConsultantId INT,
                ProjectId INT,
                StartDate DATE,
                EndDate DATE
            );
        ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE SP_InsertConsultantPendingSubmissions
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
            END;
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_InsertConsultantPendingSubmissions;");

            migrationBuilder.Sql("DROP TYPE IF EXISTS ConsultantPendingSubmissionType;");
        }
    }
}
