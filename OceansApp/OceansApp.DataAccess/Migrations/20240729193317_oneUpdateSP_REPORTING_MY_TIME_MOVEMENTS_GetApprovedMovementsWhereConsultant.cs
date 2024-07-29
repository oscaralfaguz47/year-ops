using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class oneUpdateSP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant : Migration
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
                M.Quantity
                ,MT.Name
            FROM REPORTING_MY_TIME_MOVEMENTS M
            INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
            INNER JOIN TRANSACTION_STATUSES TS ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE TS.Name = 'Approved'
            AND M.ConsultantId = @ConsultantId 
            AND M.ProjectId = @ProjectId
            AND M.ActionDate BETWEEN @StartDate AND @EndDate
            AND MT.IsPayable = 1
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_SHEETS_GetApprovedProjectMovementsWhereConsultant");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetApprovedProjectMovementsWhereConsultant
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT,
            @ProjectId INT
            AS
            BEGIN
            SELECT
                M.Quantity
                ,MT.Name
            FROM REPORTING_MY_TIME_MOVEMENTS M
            INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
            INNER JOIN TRANSACTION_STATUSES TS ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE TS.Name = 'Approved'
            AND M.ConsultantId = @ConsultantId 
            AND M.ProjectId = @ProjectId
            AND M.ActionDate BETWEEN @StartDate AND @EndDate
            AND MT.IsPayable = 1
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
