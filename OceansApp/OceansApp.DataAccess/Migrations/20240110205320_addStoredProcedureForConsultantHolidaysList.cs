using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addStoredProcedureForConsultantHolidaysList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE GetAllConsultantHolidaysWithFilters
    @SearchText NVARCHAR(255),
    @Year INT,
    @FieldToOrder NVARCHAR(255),
    @DirectionOrder NVARCHAR(255),
    @Skip INT,
    @Take INT,
    @TotalCount INT OUTPUT
    AS
    BEGIN
    -- Count total results
    SELECT @TotalCount = COUNT(*)
    FROM CONSULTANT_HOLIDAYS CH
    JOIN Users U ON CH.CreatedBy = U.Id
    WHERE (@SearchText IS NULL OR LOWER(CH.Name) LIKE '%' + LOWER(@SearchText) + '%')
    AND (@Year IS NULL OR CH.Year = @Year);

    -- Request with pagination
    SELECT 
        CH.ConsultantHolidayId,
        CH.Year,
        CH.Name,
        CH.CreationDate,
        U.Name AS CreatedByName
    FROM 
        CONSULTANT_HOLIDAYS CH
    JOIN 
        Users U ON CH.CreatedBy = U.Id
    WHERE 
        (@SearchText IS NULL OR LOWER(CH.Name) LIKE '%' + LOWER(@SearchText) + '%')
        AND (@Year IS NULL OR CH.Year = @Year)
    ORDER BY 
        CASE WHEN @FieldToOrder = 'Year' AND @DirectionOrder = 'ASC' THEN CH.Year END ASC,
        CASE WHEN @FieldToOrder = 'Year' AND @DirectionOrder = 'DESC' THEN CH.Year END DESC,
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'ASC' THEN CH.Name END ASC,
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'DESC' THEN CH.Name END DESC,
        CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'DESC' THEN CH.CreationDate END DESC,
        CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'ASC' THEN CH.CreationDate END ASC,
        CH.Year
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
    END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetAllConsultantHolidaysWithFilters");
        }
    }
}
