using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twoUpdateSP_REPORTING_MY_TIME_MOVEMENTS_GetBillableHoursByClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_MOVEMENTS_GetBillableHoursByClient
             @ClientId INT,
             @StartDate DATE,
             @EndDate DATE
             AS
             BEGIN
             SET NOCOUNT ON;
             WITH LatestHistory AS (
             SELECT 
                 PCA.ProjectId,
                 PCA.ConsultantId,
                 PCAH.HourlyClientRate,
                 PCAH.PositionId,
                 ROW_NUMBER() OVER (PARTITION BY PCA.ProjectId, PCA.ConsultantId 
                                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS RowNum
             FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
             INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH 
                 ON PCA.ProjectConsultantAssignedId = PCAH.ProjectConsultantAssignedId
             WHERE PCAH.ActionDate <= @EndDate
             ),
             OvertimeType AS (
                 SELECT MovementTypeId FROM REPORTING_MY_TIME_MOVEMENT_TYPES WHERE Name = 'Overtime Hours'
             ),
             MovementBase AS (
                 SELECT 
                     RMTM.ProjectId,
                     RMTM.ConsultantId,
                     RMTM.MovementTypeId,
                     MT.Name AS MovementTypeName,
                     SUM(RMTM.Quantity) AS TotalHours,
                     PR.ClientId,
                     C.CompanyId,
                     C.LimitNumHoursForOverTime,
                     C.OverTimeAmount
                 FROM REPORTING_MY_TIME_MOVEMENTS RMTM
                 INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON RMTM.MovementTypeId = MT.MovementTypeId
                 INNER JOIN TRANSACTION_STATUSES TS ON RMTM.TransactionStatusId = TS.TransactionStatusId
                 INNER JOIN PROJECTS PR ON RMTM.ProjectId = PR.ProjectId
                 INNER JOIN CLIENT C ON PR.ClientId = C.ClientId
                 WHERE 
                     PR.ClientId = @ClientId
                     AND RMTM.IsBillable = 1
                     AND TS.Name = 'Approved'
                     AND RMTM.ActionDate BETWEEN @StartDate AND @EndDate
                     AND RMTM.Quantity > 0
                 GROUP BY 
                     RMTM.ProjectId, RMTM.ConsultantId, RMTM.MovementTypeId,
                     MT.Name, PR.ClientId, C.CompanyId, C.LimitNumHoursForOverTime, C.OverTimeAmount
             ),
             OvertimeGenerated AS (
                 SELECT 
                     MB.ProjectId,
                     MB.ConsultantId,
                     OT.MovementTypeId,
                     'Overtime Hours' AS MovementTypeName,
                     MB.TotalHours - MB.LimitNumHoursForOverTime AS TotalHours,
                     MB.ClientId,
                     MB.CompanyId,
                     MB.LimitNumHoursForOverTime,
                     MB.OverTimeAmount
                 FROM MovementBase MB
                 CROSS APPLY OvertimeType OT
                 WHERE MB.MovementTypeName = 'Normal Hours'
                   AND MB.LimitNumHoursForOverTime > 0
                   AND MB.OverTimeAmount > 0
                   AND MB.TotalHours > MB.LimitNumHoursForOverTime
             ),
             CombinedMovements AS (
                 SELECT * FROM MovementBase
                 UNION ALL
                 SELECT * FROM OvertimeGenerated
             )
             
             SELECT 
                 MAX(CONCAT(U.Name, ' ', U.LastName, ' / ', CP.Name, ' (', MB.MovementTypeName, ')')) AS ProductDescription,
                 MB.MovementTypeId,
                 MAX(CASE 
                     WHEN MB.MovementTypeName = 'Normal Hours'
                          AND MB.TotalHours > MB.LimitNumHoursForOverTime
                          AND MB.LimitNumHoursForOverTime > 0
                     THEN MB.LimitNumHoursForOverTime
                     ELSE MB.TotalHours
                 END) AS TotalHours,
                 MAX(CASE 
                     WHEN MB.MovementTypeName = 'On Call Flate Rate' THEN 500
                     WHEN MB.MovementTypeName = 'On Call Time Worked' THEN LH.HourlyClientRate * 2
                     WHEN MB.MovementTypeName = 'Overtime Hours' THEN LH.HourlyClientRate + MB.OverTimeAmount
                     ELSE LH.HourlyClientRate
                 END) AS UnitPrice,
                 MAX(CASE 
                     WHEN PCC.ProductId IS NOT NULL THEN PCC.ProductId
                     ELSE NULL
                 END) AS ProductIdConfigured,
                 MAX(CASE 
                     WHEN PCC.ProductId IS NULL THEN
                         CASE MB.MovementTypeName
                             WHEN 'Normal Hours' THEN P1.ProductId
                             WHEN 'On Call Flate Rate' THEN P2.ProductId
                             WHEN 'On Call Time Worked' THEN P3.ProductId
                             WHEN 'Overtime Hours' THEN P4.ProductId
                         END
                     ELSE NULL
                 END) AS ProductIdToConfigure,
                 MAX(CASE 
                     WHEN PCC.ProductId IS NULL THEN
                         CASE MB.MovementTypeName
                             WHEN 'Normal Hours' THEN P1.Name
                             WHEN 'On Call Flate Rate' THEN P2.Name
                             WHEN 'On Call Time Worked' THEN P3.Name
                             WHEN 'Overtime Hours' THEN P4.Name
                         END
                     ELSE NULL
                 END) AS ProductNameToConfigure,
                 MAX(CASE 
                     WHEN PCC.ProductId IS NOT NULL THEN PC.ProductCode
                     ELSE NULL
                 END) AS ProductCodeConfigured,
                 MAX(PCC.TaxPercentage) AS TaxPercentage,
                 MIN(CASE 
                     WHEN MB.MovementTypeName = 'Normal Hours' THEN 1
                     WHEN MB.MovementTypeName = 'Overtime Hours' THEN 2
                     WHEN MB.MovementTypeName = 'On Call Flate Rate' THEN 3
                     WHEN MB.MovementTypeName = 'On Call Time Worked' THEN 4
                     ELSE 99
                 END) AS OrderPriority
             
             FROM CombinedMovements MB
             LEFT JOIN LatestHistory LH 
                 ON MB.ProjectId = LH.ProjectId AND MB.ConsultantId = LH.ConsultantId AND LH.RowNum = 1
             LEFT JOIN CONSULTANT_POSITIONS CP ON LH.PositionId = CP.ConsultantPositionId
             INNER JOIN CONSULTANT_DETAILS CD ON MB.ConsultantId = CD.ConsultantId
             INNER JOIN Users U ON CD.UserId = U.Id
             LEFT JOIN PRODUCTS P1 ON P1.ProductCode = 'PR_0000001'
             LEFT JOIN PRODUCTS P2 ON P2.ProductCode = 'PR_0000002'
             LEFT JOIN PRODUCTS P3 ON P3.ProductCode = 'PR_0000003'
             LEFT JOIN PRODUCTS P4 ON P4.ProductCode = 'PR_0000004'
             LEFT JOIN PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG PCC 
                 ON PCC.ClientId = MB.ClientId 
                 AND PCC.CompanyId = MB.CompanyId 
                 AND PCC.MovementTypeId = MB.MovementTypeId
                 AND (
                     MB.MovementTypeName <> 'Overtime Hours' OR PCC.ProductId = P4.ProductId
                 )
             LEFT JOIN PRODUCTS PC ON PCC.ProductId = PC.ProductId
             GROUP BY 
                 MB.ConsultantId, MB.MovementTypeId, MB.MovementTypeName,
                 MB.TotalHours, MB.OverTimeAmount, MB.LimitNumHoursForOverTime,
                 U.Name, U.LastName
             ORDER BY 
                 MAX(U.Name), 
                 MAX(U.LastName), 
                 OrderPriority;
             END;";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_MOVEMENTS_GetBillableHoursByClient");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_MOVEMENTS_GetBillableHoursByClient
             @ClientId INT,
             @StartDate DATE,
             @EndDate DATE
             AS
             BEGIN
             SET NOCOUNT ON;
             WITH LatestHistory AS (
             SELECT 
                     PCA.ProjectId,
                     PCA.ConsultantId,
                     PCAH.HourlyClientRate,
                     PCAH.PositionId,
                     ROW_NUMBER() OVER (PARTITION BY PCA.ProjectId, PCA.ConsultantId 
                                        ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS RowNum
                 FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                 INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH 
                     ON PCA.ProjectConsultantAssignedId = PCAH.ProjectConsultantAssignedId
                 WHERE PCAH.ActionDate <= @EndDate
             ),
             OvertimeType AS (
                 SELECT MovementTypeId FROM REPORTING_MY_TIME_MOVEMENT_TYPES WHERE Name = 'Overtime Hours'
             ),
             MovementBase AS (
                 SELECT 
                     RMTM.ProjectId,
                     RMTM.ConsultantId,
                     RMTM.MovementTypeId,
                     MT.Name AS MovementTypeName,
                     SUM(RMTM.Quantity) AS TotalHours,
                     PR.ClientId,
                     C.CompanyId,
                     C.LimitNumHoursForOverTime,
                     C.OverTimeAmount
                 FROM REPORTING_MY_TIME_MOVEMENTS RMTM
                 INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON RMTM.MovementTypeId = MT.MovementTypeId
                 INNER JOIN TRANSACTION_STATUSES TS ON RMTM.TransactionStatusId = TS.TransactionStatusId
                 INNER JOIN PROJECTS PR ON RMTM.ProjectId = PR.ProjectId
                 INNER JOIN CLIENT C ON PR.ClientId = C.ClientId
                 WHERE 
                     PR.ClientId = @ClientId
                     AND RMTM.IsBillable = 1
                     AND TS.Name = 'Approved'
                     AND RMTM.ActionDate BETWEEN @StartDate AND @EndDate
                     AND RMTM.Quantity > 0
                 GROUP BY 
                     RMTM.ProjectId, RMTM.ConsultantId, RMTM.MovementTypeId,
                     MT.Name, PR.ClientId, C.CompanyId, C.LimitNumHoursForOverTime, C.OverTimeAmount
             ),
             OvertimeGenerated AS (
                 SELECT 
                     MB.ProjectId,
                     MB.ConsultantId,
                     OT.MovementTypeId,
                     'Overtime Hours' AS MovementTypeName,
                     MB.TotalHours - MB.LimitNumHoursForOverTime AS TotalHours,
                     MB.ClientId,
                     MB.CompanyId,
                     MB.LimitNumHoursForOverTime,
                     MB.OverTimeAmount
                 FROM MovementBase MB
                 CROSS APPLY OvertimeType OT
                 WHERE MB.MovementTypeName = 'Normal Hours'
                   AND MB.LimitNumHoursForOverTime > 0
                   AND MB.OverTimeAmount > 0
                   AND MB.TotalHours > MB.LimitNumHoursForOverTime
             ),
             CombinedMovements AS (
                 SELECT * FROM MovementBase
                 UNION ALL
                 SELECT * FROM OvertimeGenerated
             )
             
             SELECT 
                 MAX(CONCAT(U.Name, ' ', U.LastName, ' / ', CP.Name, ' (', MB.MovementTypeName, ')')) AS ProductDescription,
                 MB.MovementTypeId,
                 MAX(CASE 
                     WHEN MB.MovementTypeName = 'Normal Hours'
                          AND MB.TotalHours > MB.LimitNumHoursForOverTime
                          AND MB.LimitNumHoursForOverTime > 0
                     THEN MB.LimitNumHoursForOverTime
                     ELSE MB.TotalHours
                 END) AS TotalHours,
                 MAX(CASE 
                     WHEN MB.MovementTypeName = 'On Call Flate Rate' THEN 500
                     WHEN MB.MovementTypeName = 'On Call Time Worked' THEN LH.HourlyClientRate * 2
                     WHEN MB.MovementTypeName = 'Overtime Hours' THEN LH.HourlyClientRate + MB.OverTimeAmount
                     ELSE LH.HourlyClientRate
                 END) AS UnitPrice,
                 MAX(CASE 
                     WHEN PCC.ProductId IS NOT NULL THEN PCC.ProductId
                     ELSE NULL
                 END) AS ProductIdConfigured,
                 MAX(CASE 
                     WHEN PCC.ProductId IS NULL THEN
                         CASE MB.MovementTypeName
                             WHEN 'Normal Hours' THEN P1.ProductId
                             WHEN 'On Call Flate Rate' THEN P2.ProductId
                             WHEN 'On Call Time Worked' THEN P3.ProductId
                             WHEN 'Overtime Hours' THEN P4.ProductId
                         END
                     ELSE NULL
                 END) AS ProductIdToConfigure,
                 MAX(PCC.TaxPercentage) AS TaxPercentage,
                 MAX(CASE 
                     WHEN PCC.ProductId IS NULL THEN
                         CASE MB.MovementTypeName
                             WHEN 'Normal Hours' THEN P1.Name
                             WHEN 'On Call Flate Rate' THEN P2.Name
                             WHEN 'On Call Time Worked' THEN P3.Name
                             WHEN 'Overtime Hours' THEN P4.Name
                         END
                     ELSE NULL
                 END) AS ProductNameToConfigure,
                 MIN(CASE 
                     WHEN MB.MovementTypeName = 'Normal Hours' THEN 1
                     WHEN MB.MovementTypeName = 'Overtime Hours' THEN 2
                     WHEN MB.MovementTypeName = 'On Call Flate Rate' THEN 3
                     WHEN MB.MovementTypeName = 'On Call Time Worked' THEN 4
                     ELSE 99
                 END) AS OrderPriority
             
             FROM CombinedMovements MB
             LEFT JOIN LatestHistory LH 
                 ON MB.ProjectId = LH.ProjectId AND MB.ConsultantId = LH.ConsultantId AND LH.RowNum = 1
             LEFT JOIN CONSULTANT_POSITIONS CP ON LH.PositionId = CP.ConsultantPositionId
             INNER JOIN CONSULTANT_DETAILS CD ON MB.ConsultantId = CD.ConsultantId
             INNER JOIN Users U ON CD.UserId = U.Id
             LEFT JOIN PRODUCTS P1 ON P1.ProductCode = 'PR_0000001'
             LEFT JOIN PRODUCTS P2 ON P2.ProductCode = 'PR_0000002'
             LEFT JOIN PRODUCTS P3 ON P3.ProductCode = 'PR_0000003'
             LEFT JOIN PRODUCTS P4 ON P4.ProductCode = 'PR_0000004'
             LEFT JOIN PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG PCC 
                 ON PCC.ClientId = MB.ClientId 
                 AND PCC.CompanyId = MB.CompanyId 
                 AND PCC.MovementTypeId = MB.MovementTypeId
                 AND (
                     MB.MovementTypeName <> 'Overtime Hours' OR PCC.ProductId = P4.ProductId
                 )
             GROUP BY 
                 MB.ConsultantId, MB.MovementTypeId, MB.MovementTypeName,
                 MB.TotalHours, MB.OverTimeAmount, MB.LimitNumHoursForOverTime,
                 U.Name, U.LastName
             ORDER BY 
                 MAX(U.Name), 
                 MAX(U.LastName), 
                 OrderPriority;
             END;";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_MOVEMENTS_GetBillableHoursByClient");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
