using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twoUpdateSP_CONSULTANT_POSITIONS_GetAllPositionsAccountingConfigurationWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            var sp = @"CREATE PROCEDURE SP_CONSULTANT_POSITIONS_GetAllPositionsAccountingConfigurationWithFilters
            @SearchText NVARCHAR(255),
            @MovementTypeId INT,
            @CostCenterId INT,
            @AccountingAccountId INT,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION CPAC
            RIGHT JOIN CONSULTANT_POSITIONS CP ON CPAC.PositionId = CP.ConsultantPositionId
            LEFT JOIN COST_CENTER CC ON CPAC.CostCenterId = CC.CostCenterId
            LEFT JOIN ACCOUNTING_ACCOUNT AA ON CPAC.AccountingAccountId = AA.AccountingAccountId
            LEFT JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON CPAC.MovementTypeId = MT.MovementTypeId
            WHERE (@SearchText IS NULL 
              OR LOWER(CP.Name) LIKE '%' + LOWER(@SearchText) + '%')
            AND (@MovementTypeId IS NULL OR CPAC.MovementTypeId = @MovementTypeId)
            AND (@CostCenterId IS NULL OR CPAC.CostCenterId = @CostCenterId)
            AND (@AccountingAccountId IS NULL OR CPAC.AccountingAccountId = @AccountingAccountId);

            -- Request with pagination
            SELECT
          	  CPAC.Id
              ,CP.ConsultantPositionId
          	  ,CP.Name AS PositionName
          	  ,CP.IsAdministrative As IsPositionAdministrative
          	  ,MT.Name AS MovementTypeName
              ,CPAC.CompanyId
              ,CC.CostCenterCode
          	  ,CC.Description AS CostCenterName
              ,AA.AccountingAccountCode
          	  ,AA.Description AS AccountingAccountName
            FROM CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION CPAC
            RIGHT JOIN CONSULTANT_POSITIONS CP ON CPAC.PositionId = CP.ConsultantPositionId
            LEFT JOIN COST_CENTER CC ON CPAC.CostCenterId = CC.CostCenterId
            LEFT JOIN ACCOUNTING_ACCOUNT AA ON CPAC.AccountingAccountId = AA.AccountingAccountId
            LEFT JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON CPAC.MovementTypeId = MT.MovementTypeId
            WHERE (@SearchText IS NULL 
              OR LOWER(CP.Name) LIKE '%' + LOWER(@SearchText) + '%')
          	AND (@MovementTypeId IS NULL OR CPAC.MovementTypeId = @MovementTypeId)
          	AND (@CostCenterId IS NULL OR CPAC.CostCenterId = @CostCenterId)
          	AND (@AccountingAccountId IS NULL OR CPAC.AccountingAccountId = @AccountingAccountId)
            ORDER BY
            CASE WHEN @FieldToOrder = 'PositionName' AND @DirectionOrder = 'ASC' THEN CP.Name END ASC,
            CASE WHEN @FieldToOrder = 'PositionName' AND @DirectionOrder = 'DESC' THEN CP.Name END DESC,
            CASE WHEN @FieldToOrder = 'IsPositionAdministrative' AND @DirectionOrder = 'ASC' THEN CP.IsAdministrative END ASC,
            CASE WHEN @FieldToOrder = 'IsPositionAdministrative' AND @DirectionOrder = 'DESC' THEN CP.IsAdministrative END DESC,
            CASE WHEN @FieldToOrder = 'MovementTypeName' AND @DirectionOrder = 'ASC' THEN MT.Name END ASC,
            CASE WHEN @FieldToOrder = 'MovementTypeName' AND @DirectionOrder = 'DESC' THEN MT.Name END DESC,
            CASE WHEN @FieldToOrder = 'CompanyId' AND @DirectionOrder = 'ASC' THEN CPAC.CompanyId END ASC,
            CASE WHEN @FieldToOrder = 'CompanyId' AND @DirectionOrder = 'DESC' THEN CPAC.CompanyId END DESC,
            CASE WHEN @FieldToOrder = 'CostCenterCode' AND @DirectionOrder = 'ASC' THEN CC.CostCenterCode END ASC,
            CASE WHEN @FieldToOrder = 'CostCenterCode' AND @DirectionOrder = 'DESC' THEN CC.CostCenterCode END DESC,
            CASE WHEN @FieldToOrder = 'CostCenterName' AND @DirectionOrder = 'ASC' THEN CC.Description END ASC,
            CASE WHEN @FieldToOrder = 'CostCenterName' AND @DirectionOrder = 'DESC' THEN CC.Description END DESC,
            CASE WHEN @FieldToOrder = 'AccountingAccountCode' AND @DirectionOrder = 'ASC' THEN AA.AccountingAccountCode END ASC,
            CASE WHEN @FieldToOrder = 'AccountingAccountCode' AND @DirectionOrder = 'DESC' THEN AA.AccountingAccountCode END DESC,
            CASE WHEN @FieldToOrder = 'AccountingAccountName' AND @DirectionOrder = 'ASC' THEN AA.Description END ASC,
            CASE WHEN @FieldToOrder = 'AccountingAccountName' AND @DirectionOrder = 'DESC' THEN AA.Description END DESC,
            CP.Name, CPAC.CompanyId
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";
            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_POSITIONS_GetAllPositionsAccountingConfigurationWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_POSITIONS_GetAllPositionsAccountingConfigurationWithFilters
            @SearchText NVARCHAR(255),
            @MovementTypeId INT,
            @CostCenterId INT,
            @AccountingAccountId INT,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION CPAC
            RIGHT JOIN CONSULTANT_POSITIONS CP ON CPAC.PositionId = CP.ConsultantPositionId
            LEFT JOIN COST_CENTER CC ON CPAC.CostCenterId = CC.CostCenterId
            LEFT JOIN ACCOUNTING_ACCOUNT AA ON CPAC.AccountingAccountId = AA.AccountingAccountId
            LEFT JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON CPAC.MovementTypeId = MT.MovementTypeId
            WHERE (@SearchText IS NULL 
              OR LOWER(CP.Name) LIKE '%' + LOWER(@SearchText) + '%')
            AND (@MovementTypeId IS NULL OR CPAC.MovementTypeId = @MovementTypeId)
            AND (@CostCenterId IS NULL OR CPAC.CostCenterId = @CostCenterId)
            AND (@AccountingAccountId IS NULL OR CPAC.AccountingAccountId = @AccountingAccountId);

            -- Request with pagination
            SELECT
          	  CPAC.Id
              ,CP.ConsultantPositionId
          	  ,CP.Name AS PositionName
          	  ,CP.IsAdministrative As IsPositionAdministrative
          	  ,MT.Name AS MovementTypeName
              ,CPAC.CompanyId
              ,CC.CostCenterCode
          	  ,CC.Description AS CostCenterName
              ,AA.AccountingAccountCode
          	  ,AA.Description AS AccountingAccountName
            FROM CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION CPAC
            RIGHT JOIN CONSULTANT_POSITIONS CP ON CPAC.PositionId = CP.ConsultantPositionId
            LEFT JOIN COST_CENTER CC ON CPAC.CostCenterId = CC.CostCenterId
            LEFT JOIN ACCOUNTING_ACCOUNT AA ON CPAC.AccountingAccountId = AA.AccountingAccountId
            LEFT JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON CPAC.MovementTypeId = MT.MovementTypeId
            WHERE (@SearchText IS NULL 
              OR LOWER(CP.Name) LIKE '%' + LOWER(@SearchText) + '%')
          	AND (@MovementTypeId IS NULL OR CPAC.MovementTypeId = @MovementTypeId)
          	AND (@CostCenterId IS NULL OR CPAC.CostCenterId = @CostCenterId)
          	AND (@AccountingAccountId IS NULL OR CPAC.AccountingAccountId = @AccountingAccountId)
            ORDER BY
            CASE WHEN @FieldToOrder = 'PositionName' AND @DirectionOrder = 'ASC' THEN CP.Name END ASC,
            CASE WHEN @FieldToOrder = 'PositionName' AND @DirectionOrder = 'DESC' THEN CP.Name END DESC,
            CASE WHEN @FieldToOrder = 'IsPositionAdministrative' AND @DirectionOrder = 'ASC' THEN CP.IsAdministrative END ASC,
            CASE WHEN @FieldToOrder = 'IsPositionAdministrative' AND @DirectionOrder = 'DESC' THEN CP.IsAdministrative END DESC,
            CASE WHEN @FieldToOrder = 'MovementTypeName' AND @DirectionOrder = 'ASC' THEN MT.Name END ASC,
            CASE WHEN @FieldToOrder = 'MovementTypeName' AND @DirectionOrder = 'DESC' THEN MT.Name END DESC,
            CASE WHEN @FieldToOrder = 'CompanyId' AND @DirectionOrder = 'ASC' THEN CPAC.CompanyId END ASC,
            CASE WHEN @FieldToOrder = 'CompanyId' AND @DirectionOrder = 'DESC' THEN CPAC.CompanyId END DESC,
            CASE WHEN @FieldToOrder = 'CostCenterCode' AND @DirectionOrder = 'ASC' THEN CC.CostCenterCode END ASC,
            CASE WHEN @FieldToOrder = 'CostCenterCode' AND @DirectionOrder = 'DESC' THEN CC.CostCenterCode END DESC,
            CASE WHEN @FieldToOrder = 'CostCenterName' AND @DirectionOrder = 'ASC' THEN CC.Description END ASC,
            CASE WHEN @FieldToOrder = 'CostCenterName' AND @DirectionOrder = 'DESC' THEN CC.Description END DESC,
            CASE WHEN @FieldToOrder = 'AccountingAccountCode' AND @DirectionOrder = 'ASC' THEN AA.AccountingAccountCode END ASC,
            CASE WHEN @FieldToOrder = 'AccountingAccountCode' AND @DirectionOrder = 'DESC' THEN AA.AccountingAccountCode END DESC,
            CASE WHEN @FieldToOrder = 'AccountingAccountName' AND @DirectionOrder = 'ASC' THEN AA.Description END ASC,
            CASE WHEN @FieldToOrder = 'AccountingAccountName' AND @DirectionOrder = 'DESC' THEN AA.Description END DESC,
            CP.Name, MT.Name
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_POSITIONS_GetAllPositionsAccountingConfigurationWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
