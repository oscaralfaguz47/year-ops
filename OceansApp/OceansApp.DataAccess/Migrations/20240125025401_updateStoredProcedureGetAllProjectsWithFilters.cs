using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateStoredProcedureGetAllProjectsWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_GetAllProjectsWithFilters
    @SearchText NVARCHAR(255),
    @StartDate DATE,
    @EndDate DATE,
    @IsActive BIT,
    @ClientId INT,
    @SuccessManagerId INT,
    @FieldToOrder NVARCHAR(255),
    @DirectionOrder NVARCHAR(255),
    @Skip INT,
    @Take INT,
    @TotalCount INT OUTPUT
    AS
    BEGIN
    -- Count total results
    SELECT @TotalCount = COUNT(*)
    FROM PROJECTS P
        JOIN CLIENT C ON P.ClientId = C.ClientId
        LEFT JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
        LEFT JOIN Users U ON CD.UserId = U.Id
        WHERE ((@SearchText IS NULL OR LOWER(P.Name) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(P.Description) LIKE '%' + LOWER(@SearchText) + '%'))
        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (P.StartDate >= @StartDate AND P.StartDate <= @EndDate))
        AND (@IsActive IS NULL OR P.IsActive = @IsActive)
        AND (@ClientId IS NULL OR P.ClientId = @ClientId)
        AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId);

    -- Request with pagination
    SELECT P.ProjectId
      ,P.Name
      ,P.Description
      ,P.StartDate
      ,P.IsActive
	  ,C.Name as ClientName
	  ,U.Name + ' ' + U.LastName AS SuccessManagerName
      ,(SELECT COUNT(*) FROM PROJECTS_CONSULTANTS_ASSIGNED WHERE ProjectId = P.ProjectId) AS NumConsultantsAssigned
       FROM PROJECTS P
       JOIN CLIENT C ON P.ClientId = C.ClientId
       LEFT JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
       LEFT JOIN Users U ON CD.UserId = U.Id
       WHERE ((@SearchText IS NULL OR LOWER(P.Name) LIKE '%' + LOWER(@SearchText) + '%')
       OR (@SearchText IS NULL OR LOWER(P.Description) LIKE '%' + LOWER(@SearchText) + '%'))
       AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (P.StartDate >= @StartDate AND P.StartDate <= @EndDate))
       AND (@IsActive IS NULL OR P.IsActive = @IsActive)
       AND (@ClientId IS NULL OR P.ClientId = @ClientId)
       AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId)
        ORDER BY 
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'ASC' THEN P.Name END ASC,
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'DESC' THEN P.Name END DESC,
        CASE WHEN @FieldToOrder = 'Description' AND @DirectionOrder = 'ASC' THEN P.Description END ASC,
        CASE WHEN @FieldToOrder = 'Description' AND @DirectionOrder = 'DESC' THEN P.Description END DESC,
        CASE WHEN @FieldToOrder = 'StartDate' AND @DirectionOrder = 'DESC' THEN P.StartDate END DESC,
        CASE WHEN @FieldToOrder = 'StartDate' AND @DirectionOrder = 'ASC' THEN P.StartDate END ASC,
		CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'DESC' THEN C.Name END DESC,
        CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'ASC' THEN C.Name END ASC,
		CASE WHEN @FieldToOrder = 'SuccessManagerName' AND @DirectionOrder = 'DESC' THEN U.Name END DESC,
        CASE WHEN @FieldToOrder = 'SuccessManagerName' AND @DirectionOrder = 'ASC' THEN U.Name END ASC,
        P.StartDate DESC
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetAllProjectsWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE GetAllProjectsWithFilters
    @SearchText NVARCHAR(255),
    @StartDate DATE,
    @EndDate DATE,
    @IsActive BIT,
    @ClientId INT,
    @SuccessManagerId INT,
    @FieldToOrder NVARCHAR(255),
    @DirectionOrder NVARCHAR(255),
    @Skip INT,
    @Take INT,
    @TotalCount INT OUTPUT
    AS
    BEGIN
    -- Count total results
    SELECT @TotalCount = COUNT(*)
    FROM PROJECTS P
        JOIN CLIENT C ON P.ClientId = C.ClientId
        LEFT JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
        LEFT JOIN Users U ON CD.UserId = U.Id
        WHERE ((@SearchText IS NULL OR LOWER(P.Name) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(P.Description) LIKE '%' + LOWER(@SearchText) + '%'))
        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (P.StartDate >= @StartDate AND P.StartDate <= @EndDate))
        AND (@IsActive IS NULL OR P.IsActive = @IsActive)
        AND (@ClientId IS NULL OR P.ClientId = @ClientId)
        AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId);

    -- Request with pagination
    SELECT P.ProjectId
      ,P.Name
      ,P.Description
      ,P.StartDate
      ,P.IsActive
	  ,C.Name as ClientName
	  ,U.Name + ' ' + U.LastName AS SuccessManagerName
       FROM PROJECTS P
       JOIN CLIENT C ON P.ClientId = C.ClientId
       LEFT JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
       LEFT JOIN Users U ON CD.UserId = U.Id
       WHERE ((@SearchText IS NULL OR LOWER(P.Name) LIKE '%' + LOWER(@SearchText) + '%')
       OR (@SearchText IS NULL OR LOWER(P.Description) LIKE '%' + LOWER(@SearchText) + '%'))
       AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (P.StartDate >= @StartDate AND P.StartDate <= @EndDate))
       AND (@IsActive IS NULL OR P.IsActive = @IsActive)
       AND (@ClientId IS NULL OR P.ClientId = @ClientId)
       AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId)
        ORDER BY 
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'ASC' THEN P.Name END ASC,
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'DESC' THEN P.Name END DESC,
        CASE WHEN @FieldToOrder = 'Description' AND @DirectionOrder = 'ASC' THEN P.Description END ASC,
        CASE WHEN @FieldToOrder = 'Description' AND @DirectionOrder = 'DESC' THEN P.Description END DESC,
        CASE WHEN @FieldToOrder = 'StartDate' AND @DirectionOrder = 'DESC' THEN P.StartDate END DESC,
        CASE WHEN @FieldToOrder = 'StartDate' AND @DirectionOrder = 'ASC' THEN P.StartDate END ASC,
		CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'DESC' THEN C.Name END DESC,
        CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'ASC' THEN C.Name END ASC,
		CASE WHEN @FieldToOrder = 'SuccessManagerName' AND @DirectionOrder = 'DESC' THEN U.Name END DESC,
        CASE WHEN @FieldToOrder = 'SuccessManagerName' AND @DirectionOrder = 'ASC' THEN U.Name END ASC,
        P.StartDate DESC
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_GetAllProjectsWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
