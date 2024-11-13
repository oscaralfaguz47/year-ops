using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            WITH MovementType AS (
                SELECT MovementTypeId, Name
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES
                WHERE Name = 'Interviews'
            )
            
            SELECT
                SUM(I.DurationMinutes) / 60.0 AS TotalDurationHours,
                MT.MovementTypeId,
                CONCAT(MT.Name, ' (', COUNT(I.DurationMinutes), ')') AS MovementTypeName
            FROM INTERVIEWS I
            INNER JOIN TRANSACTION_STATUSES TS ON I.TransactionStatusId = TS.TransactionStatusId
            CROSS JOIN MovementType MT
            WHERE I.ConsultantId = @ConsultantId
              AND I.Date BETWEEN @StartDate AND @EndDate
              AND TS.Name = 'Approved'
            GROUP BY MT.MovementTypeId, MT.Name;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod");
        }
    }
}
