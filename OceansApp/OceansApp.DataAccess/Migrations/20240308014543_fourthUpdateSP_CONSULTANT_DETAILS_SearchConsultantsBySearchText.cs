using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class fourthUpdateSP_CONSULTANT_DETAILS_SearchConsultantsBySearchText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText
            @SearchText NVARCHAR(100),
            @UserCategoryName NVARCHAR(50)
            AS
            BEGIN
            SELECT 
            C.ConsultantId
            ,U.Name + ' ' + U.LastName AS ConsultantName
            ,U.Email
            ,UC.Name AS UserCategoryName
            FROM CONSULTANT_DETAILS C
            JOIN Users U ON C.UserId = U.Id
            JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            WHERE ((@SearchText IS NULL OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
            OR (@SearchText IS NULL OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%')
            OR (@SearchText IS NULL OR LOWER(U.Email) LIKE '%' + LOWER(@SearchText) + '%'))
            AND (@UserCategoryName IS NULL OR UC.Name = @UserCategoryName)
            AND U.IsActive = 1
            ORDER BY U.Name
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText
            @SearchText NVARCHAR(100)
            AS
            BEGIN
            SELECT 
            C.ConsultantId
            ,U.Name + ' ' + U.LastName AS ConsultantName
            ,U.Email
            ,UC.Name AS UserCategoryName
            FROM CONSULTANT_DETAILS C
            JOIN Users U ON C.UserId = U.Id
            JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
            WHERE ((@SearchText IS NULL OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
            OR (@SearchText IS NULL OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%')
            OR (@SearchText IS NULL OR LOWER(U.Email) LIKE '%' + LOWER(@SearchText) + '%'))
            AND U.IsActive = 1
            ORDER BY U.Name
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
