using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateStoredProcedureGetNumOfUsersByCategoryConsultantIdAndPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE GetNumOfUsersByCategoryConsultantIdAndPosition
                       @UserCategory NVARCHAR(50),
                       @UserPosition NVARCHAR(100),
                       @ConsultantId INT
                       AS
                       BEGIN
                       SELECT
                       COUNT(*)
                       FROM CONSULTANTS_AND_POSITIONS CAP
                       JOIN CONSULTANT_DETAILS CD ON CAP.ConsultantId = CD.ConsultantId
                       JOIN Users U ON CD.UserId = U.Id
                       JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
                       JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                       WHERE CP.Name = @UserPosition
                       AND UC.Name = @UserCategory
                       AND CAP.ConsultantId = @ConsultantId
                       END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetNumOfUsersByCategoryConsultantIdAndPosition");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE GetNumOfUsersByCategoryConsultantIdAndPosition
                       @UserCategory NVARCHAR(50),
                       @UserPosition NVARCHAR(100),
                       @ConsultantId INT
                       AS
                       BEGIN
                       SELECT
                       COUNT(*)
                       FROM CONSULTANT_DETAILS C
                       JOIN Users U ON C.UserId = U.Id
                       JOIN CONSULTANT_POSITIONS CP ON C.ConsultantPositionId = CP.ConsultantPositionId
                       JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
                       WHERE CP.Name = @UserPosition
                       AND UC.Name = @UserCategory
                       AND C.ConsultantId = @ConsultantId
                       END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetNumOfUsersByCategoryConsultantIdAndPosition");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
