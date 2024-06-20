using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_CONSULTANT_POSITIONS_GetCompanyMovementTypeByPositionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            var sp = @"CREATE PROCEDURE SP_CONSULTANT_POSITIONS_GetCompanyMovementTypeByPositionId
            @PositionId INT
            AS
            BEGIN
            SELECT 
                CPAC.Id,
                C.CompanyId AS CompanyId,
                C.Name AS CompanyName,
                CPAC.CostCenterId,
                CC.Description AS CostCenterName,
                CPAC.AccountingAccountId,
                AA.Description AS AccountingAccountName,
                MT.MovementTypeId,
                MT.Name AS MovementTypeName
            FROM 
                COMPANIES C
            CROSS JOIN 
                REPORTING_MY_TIME_MOVEMENT_TYPES MT
            LEFT JOIN 
                CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION CPAC 
                ON C.CompanyId = CPAC.CompanyId 
                AND MT.MovementTypeId = CPAC.MovementTypeId
                AND (CPAC.PositionId = @PositionId OR CPAC.PositionId IS NULL)
            LEFT JOIN 
                COST_CENTER CC ON CPAC.CostCenterId = CC.CostCenterId
            LEFT JOIN 
                ACCOUNTING_ACCOUNT AA ON CPAC.AccountingAccountId = AA.AccountingAccountId
            ORDER BY 
                C.CompanyId, MT.Name;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_POSITIONS_GetCompanyMovementTypeByPositionId");
        }
    }
}
