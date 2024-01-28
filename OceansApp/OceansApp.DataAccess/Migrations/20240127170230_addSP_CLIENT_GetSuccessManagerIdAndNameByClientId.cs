using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_CLIENT_GetSuccessManagerIdAndNameByClientId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CLIENT_GetSuccessManagerIdAndNameByClientId
            @ClientId INT
            AS
            BEGIN
            SELECT 
            U.Name + ' ' + U.LastName AS SuccessManagerName
            ,CD.ConsultantId AS SuccessManagerId
            FROM CONSULTANT_DETAILS CD
            JOIN CLIENT C ON CD.ConsultantId = C.SuccessManager
            JOIN Users U ON CD.UserId = U.Id
            JOIN CONSULTANTS_AND_POSITIONS CAP ON CD.ConsultantId = CAP.ConsultantId
            JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
            WHERE CP.Name = 'Success Manager'
            AND C.ClientId = @ClientId
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CLIENT_GetSuccessManagerIdAndNameByClientId");
        }
    }
}
