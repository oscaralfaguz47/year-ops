using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_CONSULTANT_POSITIONS_GetPositionsByConsultantId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_POSITIONS_GetPositionsByConsultantId
                       @ConsultantId INT
                       AS
                       BEGIN
                       SELECT 
                       CAP.ConsultantPositionId,
                       CP.Name AS PositionName
                       FROM CONSULTANTS_AND_POSITIONS CAP
                       INNER JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
                       WHERE CAP.ConsultantId = @ConsultantId
                       ORDER BY CP.Name
                       END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_POSITIONS_GetPositionsByConsultantId");
        }
    }
}
