using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_REPORTIN_MY_TIME_GetProjectMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_GetProjectMovements
            @ProjectId INT,
            @ConsultantId INT,
            @StartActionDate DATE,
            @FinalActionDate DATE
            AS
            BEGIN
            SELECT 
            M.MovementId,
            MT.Name AS MovementTypeName,
            M.Quantity,
            M.Notes,
            M.ActionDate,
            TS.Name AS TransactionStatus,
            ISNULL(B.BlobNames, '[]') AS BlobNames
            FROM REPORTING_MY_TIME_MOVEMENTS M
            INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
            INNER JOIN TRANSACTION_STATUSES TS ON M.TransactionStatusId = TS.TransactionStatusId
            OUTER APPLY (
              SELECT ('[' + STRING_AGG('""' + REPLACE(B.BlobName, '""', '\""') + '""', ',') + ']') AS BlobNames
              FROM REPORTING_MY_TIME_MOVEMENT_BLOBS B
              WHERE B.MovementId = M.MovementId
            ) B
            WHERE M.ProjectId = @ProjectId
            AND M.ConsultantId = @ConsultantId
            AND (M.ActionDate >= @StartActionDate AND M.ActionDate <= @FinalActionDate)
            ORDER BY MT.Name;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_GetProjectMovements");
        }
    }
}
