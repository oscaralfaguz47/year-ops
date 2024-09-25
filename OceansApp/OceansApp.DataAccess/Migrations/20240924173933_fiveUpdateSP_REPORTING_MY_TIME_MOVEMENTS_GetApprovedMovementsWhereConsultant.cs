using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fiveUpdateSP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT,
            @ProjectId INT
            AS
            BEGIN
            SELECT
                  M.MovementId,
                  MT.MovementTypeId,
                  MT.Name AS MovementTypeName,
                  SUM(M.Quantity) AS TotalQuantity
              FROM REPORTING_MY_TIME_MOVEMENTS M
              INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
              INNER JOIN TRANSACTION_STATUSES TS ON M.TransactionStatusId = TS.TransactionStatusId
              WHERE TS.Name <> 'Rejected'
              AND M.ConsultantId = @ConsultantId 
              AND M.ProjectId = @ProjectId
              AND M.ActionDate BETWEEN @StartDate AND @EndDate
              AND MT.IsPayable = 1
            GROUP BY M.MovementId, MT.MovementTypeId, MT.Name
            ORDER BY MT.Name;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT,
            @ProjectId INT
            AS
            BEGIN
            SELECT
                  M.MovementId,
                  MT.MovementTypeId,
                  MT.Name AS MovementTypeName,
                  SUM(M.Quantity) AS TotalQuantity
              FROM REPORTING_MY_TIME_MOVEMENTS M
              INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
              INNER JOIN TRANSACTION_STATUSES TS ON M.TransactionStatusId = TS.TransactionStatusId
              WHERE TS.Name <> 'Rejected'
              AND M.ConsultantId = @ConsultantId 
              AND M.ProjectId = @ProjectId
              AND M.ActionDate BETWEEN @StartDate AND @EndDate
              AND MT.IsPayable = 1
            GROUP BY M.MovementId, MT.MovementTypeId, MT.Name;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
