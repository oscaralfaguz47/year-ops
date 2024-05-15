using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class thirdUpdateSPGetUsersByCategoryAndPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE GetUsersByCategoryAndPosition
                       @UserCategory NVARCHAR(50),
                       @UserPosition NVARCHAR(100)
                       AS
                       BEGIN
                       SELECT
                       CAP.ConsultantId as UserId,
                       U.Name + ' ' + U.LastName AS UserName
                       FROM CONSULTANTS_AND_POSITIONS CAP
					   JOIN CONSULTANT_DETAILS CD ON CAP.ConsultantId = CD.ConsultantId
                       JOIN Users U ON CD.UserId = U.Id
                       JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
                       JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                       WHERE CP.Name = @UserPosition
                       AND UC.Name = @UserCategory
                       AND U.IsActive = 1
                       ORDER BY U.Name
                       END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetUsersByCategoryAndPosition");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE GetUsersByCategoryAndPosition
                       @UserCategory NVARCHAR(50),
                       @UserPosition NVARCHAR(100)
                       AS
                       BEGIN
                       SELECT
                       CAP.ConsultantId as UserId,
                       U.Name + ' ' + U.LastName AS UserName
                       FROM CONSULTANTS_AND_POSITIONS CAP
					   JOIN CONSULTANT_DETAILS CD ON CAP.ConsultantId = CD.ConsultantId
                       JOIN Users U ON CD.UserId = U.Id
                       JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
                       JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                       WHERE CP.Name = @UserPosition
                       AND UC.Name = @UserCategory
                       ORDER BY U.Name
                       END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetUsersByCategoryAndPosition");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
