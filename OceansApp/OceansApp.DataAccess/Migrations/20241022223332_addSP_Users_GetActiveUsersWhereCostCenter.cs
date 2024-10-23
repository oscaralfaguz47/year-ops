using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_Users_GetActiveUsersWhereCostCenter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_Users_GetActiveUsersWhereCostCenter
             @CostCenterCodes VARCHAR(50)
             AS
             BEGIN
             SET NOCOUNT ON;
             WITH ConsultantDetailsCTE AS (
                 SELECT 
                     CONCAT(u.Name, ' ', u.LastName) AS ConsultantName,
                     u.Email,
                     u.PhoneNumber,
                     cc.Description AS AreaName,
                     po.Name AS PositionName,
             		ib.BlobUrl AS ProfileUrl
                 FROM 
                     CONSULTANT_DETAILS cd
                 INNER JOIN 
                     Users u ON cd.UserId = u.Id
             	LEFT JOIN IMAGE_BLOBS ib ON u.Id = ib.EntityId 
             	AND ib.ContainerName = 'user-profile-photos' 
             	AND ib.EntityType = 'UserProfile'
                 INNER JOIN 
                     CONSULTANTS_AND_POSITIONS cp 
                     ON cd.ConsultantId = cp.ConsultantId
                 INNER JOIN 
                     CONSULTANT_POSITIONS po 
                     ON cp.ConsultantPositionId = po.ConsultantPositionId
                 INNER JOIN 
                     CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION cpac 
                     ON cp.ConsultantPositionId = cpac.PositionId
                 INNER JOIN 
                     COST_CENTER cc 
                     ON cpac.CostCenterId = cc.CostCenterId
                 INNER JOIN 
                     REPORTING_MY_TIME_MOVEMENT_TYPES rmtmt
                     ON cpac.MovementTypeId = rmtmt.MovementTypeId
                 WHERE 
                     cc.CostCenterCode IN (SELECT value FROM STRING_SPLIT(@CostCenterCodes, ','))
                     AND rmtmt.Name = 'Normal Hours'
                     AND u.IsActive = 1
             )
             SELECT DISTINCT
                 AreaName,
                 PositionName,
                 ConsultantName,
                 Email,
                 PhoneNumber,
             	ProfileUrl
             FROM 
                 ConsultantDetailsCTE
             ORDER BY 
                 AreaName, PositionName, ConsultantName;
             END;
             ";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_Users_GetActiveUsersWhereCostCenter");
        }
    }
}
