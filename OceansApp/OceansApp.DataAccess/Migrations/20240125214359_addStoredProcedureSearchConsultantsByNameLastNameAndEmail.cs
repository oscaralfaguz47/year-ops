using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addStoredProcedureSearchConsultantsByNameLastNameAndEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText
            @SearchText NVARCHAR(100)
            AS
            BEGIN
            SELECT 
            U.Name + ' ' + U.LastName
            ,U.Email
            FROM CONSULTANT_DETAILS C
            JOIN Users U ON C.UserId = U.Id
            WHERE ((@SearchText IS NULL OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%')
            OR (@SearchText IS NULL OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%')
            OR (@SearchText IS NULL OR LOWER(U.Email) LIKE '%' + LOWER(@SearchText) + '%'))
            AND U.IsActive = 1
            ORDER BY U.Name
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText");
        }
    }
}
