using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateSP_CONSULTANT_HOLIDAYS_GetAllConsultantHolidaysWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_HOLIDAYS_GetAllConsultantHolidaysWithFilters
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
        U.Name AS CreatedByName,
		(SELECT COUNT(*) FROM CONSULTANT_HOLIDAY_DATES 
         WHERE ConsultantHolidayId = CH.ConsultantHolidayId) AS NumHolidays
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

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetAllConsultantHolidaysWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE GetAllConsultantHolidaysWithFilters
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

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_HOLIDAYS_GetAllConsultantHolidaysWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
