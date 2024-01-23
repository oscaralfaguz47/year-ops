using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addStoredProcedureGetUsersByCategoryAndPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE GetUsersByCategoryAndPosition
                       @UserCategory NVARCHAR(50),
                       @UserPosition NVARCHAR(100)
                       AS
                       BEGIN
                       SELECT
                       C.ConsultantId as UserId,
                       U.Name + ' ' + U.LastName AS UserName
                       FROM CONSULTANT_DETAILS C
                       JOIN Users U ON C.UserId = U.Id
                       JOIN CONSULTANT_POSITIONS CP ON C.ConsultantPositionId = CP.ConsultantPositionId
                       JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                       WHERE CP.Name = @UserPosition
                       AND UC.Name = @UserCategory
                       ORDER BY U.Name
                       END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetUsersByCategoryAndPosition");
        }
    }
}
