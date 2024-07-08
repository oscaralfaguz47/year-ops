using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fiveUpdateSP_CONSULTANT_DETAILS_GetAllConsultantsWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_DETAILS_GetAllConsultantsWithFilters
    @SearchText NVARCHAR(255),
    @ProjectIds ConsultantProjectIdTableType READONLY,
    @PositionIds ConsultantPositionIdTableType READONLY,
    @CountryId NVARCHAR(4),
    @IsTwoFactorEnabled BIT,
    @EmailConfirmed BIT,
    @IsActive BIT,
    @UserCategoryId INT,
    @FieldToOrder NVARCHAR(255),
    @DirectionOrder NVARCHAR(255),
    @Skip INT,
    @Take INT,
    @TotalCount INT OUTPUT
    AS
    BEGIN
    -- Count total results
    SELECT @TotalCount = COUNT(*)
    FROM CONSULTANT_DETAILS CD
      JOIN COUNTRY CO ON CD.IdCountry = CO.IdCountry
      JOIN Users U ON CD.UserId = U.Id
      JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
      OUTER APPLY (
              SELECT STRING_AGG(CP.Name, '; ') AS ConsultantPositions
              FROM CONSULTANTS_AND_POSITIONS CAP
              JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
              WHERE CAP.ConsultantId = CD.ConsultantId
      ) AS Positions
           OUTER APPLY (
           SELECT 
               P.Name, 
               PA.IsActive 
           FROM 
               PROJECTS_CONSULTANTS_ASSIGNED PA
               JOIN PROJECTS P ON PA.ProjectId = P.ProjectId
           WHERE 
               PA.ConsultantId = CD.ConsultantId
           FOR JSON PATH
      ) AS Projects(ConsultantProjects)
      WHERE 
      (
       (SELECT COUNT(*) FROM @ProjectIds) = 0 
       OR CD.ConsultantId IN (
           SELECT DISTINCT PA.ConsultantId
           FROM PROJECTS_CONSULTANTS_ASSIGNED PA
           JOIN @ProjectIds POID ON PA.ProjectId = POID.ConsultantProjectId
       )
   	)
   AND(
       (SELECT COUNT(*) FROM @PositionIds) = 0 
       OR CD.ConsultantId IN (
           SELECT DISTINCT CAP.ConsultantId
           FROM CONSULTANTS_AND_POSITIONS CAP
           JOIN @PositionIds PID ON CAP.ConsultantPositionId = PID.ConsultantPositionId
       )
   )
   AND
    (@SearchText IS NULL 
    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Address) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.PersonalEmail) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.Email) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.PhoneNumber) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Phone2) LIKE '%' + LOWER(@SearchText) + '%')
	AND (@CountryId IS NULL OR CD.IdCountry = @CountryId)
	AND (@IsTwoFactorEnabled IS NULL OR U.TwoFactorEnabled = @IsTwoFactorEnabled)
	AND (@EmailConfirmed IS NULL OR U.EmailConfirmed = @EmailConfirmed)
	AND (@IsActive IS NULL OR U.IsActive = @IsActive)
	AND (@UserCategoryId IS NULL OR U.UserCategoryId = @UserCategoryId);

    -- Request with pagination
    SELECT 
      CD.ConsultantId
      ,CO.Name AS CountryName
      ,CD.Phone2
      ,CD.Address
      ,CD.PersonalEmail
      ,CD.Location
      ,CD.ShirtSize
      ,U.Name + ' ' + U.LastName AS ConsultantName
      ,U.Email AS InternalEmail
      ,U.PhoneNumber
      ,U.TwoFactorEnabled
	  ,U.TwoFactorRequired
      ,U.EmailConfirmed
      ,U.IsActive
      ,CASE 
          WHEN U.LockoutEnd > GETDATE() THEN 1
          ELSE 0
      END AS IsLockedOut
      ,UC.Name AS UserCategoryName
      ,Positions.ConsultantPositions 
      ,Projects.ConsultantProjects
      FROM CONSULTANT_DETAILS CD
      JOIN COUNTRY CO ON CD.IdCountry = CO.IdCountry
      JOIN Users U ON CD.UserId = U.Id
      JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
      OUTER APPLY (
              SELECT STRING_AGG(CP.Name, '; ') AS ConsultantPositions
              FROM CONSULTANTS_AND_POSITIONS CAP
              JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
              WHERE CAP.ConsultantId = CD.ConsultantId
      ) AS Positions
           OUTER APPLY (
           SELECT 
               P.Name, 
               PA.IsActive 
           FROM 
               PROJECTS_CONSULTANTS_ASSIGNED PA
               JOIN PROJECTS P ON PA.ProjectId = P.ProjectId
           WHERE 
               PA.ConsultantId = CD.ConsultantId
           FOR JSON PATH
      ) AS Projects(ConsultantProjects)
      WHERE 
      (
       (SELECT COUNT(*) FROM @ProjectIds) = 0 
       OR CD.ConsultantId IN (
           SELECT DISTINCT PA.ConsultantId
           FROM PROJECTS_CONSULTANTS_ASSIGNED PA
           JOIN @ProjectIds POID ON PA.ProjectId = POID.ConsultantProjectId
       )
   	)
   AND(
       (SELECT COUNT(*) FROM @PositionIds) = 0 
       OR CD.ConsultantId IN (
           SELECT DISTINCT CAP.ConsultantId
           FROM CONSULTANTS_AND_POSITIONS CAP
           JOIN @PositionIds PID ON CAP.ConsultantPositionId = PID.ConsultantPositionId
       )
   )
   AND
    (@SearchText IS NULL 
    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Address) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.PersonalEmail) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.Email) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.PhoneNumber) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Phone2) LIKE '%' + LOWER(@SearchText) + '%')
	AND (@CountryId IS NULL OR CD.IdCountry = @CountryId)
	AND (@IsTwoFactorEnabled IS NULL OR U.TwoFactorEnabled = @IsTwoFactorEnabled)
	AND (@EmailConfirmed IS NULL OR U.EmailConfirmed = @EmailConfirmed)
	AND (@IsActive IS NULL OR U.IsActive = @IsActive)
	AND (@UserCategoryId IS NULL OR U.UserCategoryId = @UserCategoryId)
	 ORDER BY 
        CASE WHEN @FieldToOrder = 'CountryName' AND @DirectionOrder = 'ASC' THEN CO.Name END ASC,
        CASE WHEN @FieldToOrder = 'CountryName' AND @DirectionOrder = 'DESC' THEN CO.Name END DESC,
		CASE WHEN @FieldToOrder = 'PersonalEmail' AND @DirectionOrder = 'ASC' THEN CD.PersonalEmail END ASC,
        CASE WHEN @FieldToOrder = 'PersonalEmail' AND @DirectionOrder = 'DESC' THEN CD.PersonalEmail END DESC,
		CASE WHEN @FieldToOrder = 'ShirtSize' AND @DirectionOrder = 'ASC' THEN CD.ShirtSize END ASC,
        CASE WHEN @FieldToOrder = 'ShirtSize' AND @DirectionOrder = 'DESC' THEN CD.ShirtSize END DESC,
		CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN U.Name + ' ' + U.LastName END ASC,
        CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN U.Name + ' ' + U.LastName END DESC,
		CASE WHEN @FieldToOrder = 'InternalEmail' AND @DirectionOrder = 'ASC' THEN U.Email END ASC,
        CASE WHEN @FieldToOrder = 'InternalEmail' AND @DirectionOrder = 'DESC' THEN U.Email END DESC,
		CASE WHEN @FieldToOrder = 'TwoFactorEnabled' AND @DirectionOrder = 'ASC' THEN U.TwoFactorEnabled END ASC,
        CASE WHEN @FieldToOrder = 'TwoFactorEnabled' AND @DirectionOrder = 'DESC' THEN U.TwoFactorEnabled END DESC,
		CASE WHEN @FieldToOrder = 'EmailConfirmed' AND @DirectionOrder = 'ASC' THEN U.EmailConfirmed END ASC,
        CASE WHEN @FieldToOrder = 'EmailConfirmed' AND @DirectionOrder = 'DESC' THEN U.EmailConfirmed END DESC,
		CASE WHEN @FieldToOrder = 'IsActive' AND @DirectionOrder = 'ASC' THEN U.IsActive END ASC,
        CASE WHEN @FieldToOrder = 'IsActive' AND @DirectionOrder = 'DESC' THEN U.IsActive END DESC,
		CASE WHEN @FieldToOrder = 'UserCategoryName' AND @DirectionOrder = 'ASC' THEN UC.Name END ASC,
        CASE WHEN @FieldToOrder = 'UserCategoryName' AND @DirectionOrder = 'DESC' THEN UC.Name END DESC,
		CD.CreationDate DESC
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_DETAILS_GetAllConsultantsWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_DETAILS_GetAllConsultantsWithFilters
    @SearchText NVARCHAR(255),
    @ProjectIds ConsultantProjectIdTableType READONLY,
    @PositionIds ConsultantPositionIdTableType READONLY,
    @CountryId NVARCHAR(4),
    @IsTwoFactorEnabled BIT,
    @EmailConfirmed BIT,
    @IsActive BIT,
    @UserCategoryId INT,
    @FieldToOrder NVARCHAR(255),
    @DirectionOrder NVARCHAR(255),
    @Skip INT,
    @Take INT,
    @TotalCount INT OUTPUT
    AS
    BEGIN
    -- Count total results
    SELECT @TotalCount = COUNT(*)
    FROM CONSULTANT_DETAILS CD
    JOIN COUNTRY CO ON CD.IdCountry = CO.IdCountry
    JOIN Users U ON CD.UserId = U.Id
    JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
    WHERE 
    (
        (SELECT COUNT(*) FROM @ProjectIds) = 0 
        OR CD.ConsultantId IN (
            SELECT DISTINCT PA.ConsultantId
            FROM PROJECTS_CONSULTANTS_ASSIGNED PA
            JOIN @ProjectIds POID ON PA.ProjectId = POID.ConsultantProjectId
        )
    	)
    AND(
    (SELECT COUNT(*) FROM @PositionIds) = 0 
    OR CD.ConsultantId IN (
        SELECT DISTINCT CAP.ConsultantId
        FROM CONSULTANTS_AND_POSITIONS CAP
        JOIN @PositionIds PID ON CAP.ConsultantPositionId = PID.ConsultantPositionId
    )
    )
    AND
    (@SearchText IS NULL 
    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Address) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.PersonalEmail) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.Email) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.PhoneNumber) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Phone2) LIKE '%' + LOWER(@SearchText) + '%')
	AND (@CountryId IS NULL OR CD.IdCountry = @CountryId)
	AND (@IsTwoFactorEnabled IS NULL OR U.TwoFactorEnabled = @IsTwoFactorEnabled)
	AND (@EmailConfirmed IS NULL OR U.EmailConfirmed = @EmailConfirmed)
	AND (@IsActive IS NULL OR U.IsActive = @IsActive)
	AND (@UserCategoryId IS NULL OR U.UserCategoryId = @UserCategoryId);

    -- Request with pagination
    SELECT 
      CD.ConsultantId
      ,CO.Name AS CountryName
      ,CD.Phone2
      ,CD.Address
      ,CD.PersonalEmail
      ,CD.Location
      ,CD.ShirtSize
      ,U.Name + ' ' + U.LastName AS ConsultantName
      ,U.Email AS InternalEmail
      ,U.PhoneNumber
      ,U.TwoFactorEnabled
      ,U.EmailConfirmed
      ,U.IsActive
      ,CASE 
          WHEN U.LockoutEnd > GETDATE() THEN 1
          ELSE 0
      END AS IsLockedOut
      ,UC.Name AS UserCategoryName
      ,Positions.ConsultantPositions 
      ,Projects.ConsultantProjects
      FROM CONSULTANT_DETAILS CD
      JOIN COUNTRY CO ON CD.IdCountry = CO.IdCountry
      JOIN Users U ON CD.UserId = U.Id
      JOIN UserCategories UC ON U.UserCategoryId = UC.UserCategoryId
      OUTER APPLY (
              SELECT STRING_AGG(CP.Name, '; ') AS ConsultantPositions
              FROM CONSULTANTS_AND_POSITIONS CAP
              JOIN CONSULTANT_POSITIONS CP ON CAP.ConsultantPositionId = CP.ConsultantPositionId
              WHERE CAP.ConsultantId = CD.ConsultantId
      ) AS Positions
           OUTER APPLY (
           SELECT 
               P.Name, 
               PA.IsActive 
           FROM 
               PROJECTS_CONSULTANTS_ASSIGNED PA
               JOIN PROJECTS P ON PA.ProjectId = P.ProjectId
           WHERE 
               PA.ConsultantId = CD.ConsultantId
           FOR JSON PATH
      ) AS Projects(ConsultantProjects)
      WHERE 
      (
       (SELECT COUNT(*) FROM @ProjectIds) = 0 
       OR CD.ConsultantId IN (
           SELECT DISTINCT PA.ConsultantId
           FROM PROJECTS_CONSULTANTS_ASSIGNED PA
           JOIN @ProjectIds POID ON PA.ProjectId = POID.ConsultantProjectId
       )
   	)
   AND(
       (SELECT COUNT(*) FROM @PositionIds) = 0 
       OR CD.ConsultantId IN (
           SELECT DISTINCT CAP.ConsultantId
           FROM CONSULTANTS_AND_POSITIONS CAP
           JOIN @PositionIds PID ON CAP.ConsultantPositionId = PID.ConsultantPositionId
       )
   )
   AND
    (@SearchText IS NULL 
    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Address) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.PersonalEmail) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.Email) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(U.PhoneNumber) LIKE '%' + LOWER(@SearchText) + '%'
	OR @SearchText IS NULL OR LOWER(CD.Phone2) LIKE '%' + LOWER(@SearchText) + '%')
	AND (@CountryId IS NULL OR CD.IdCountry = @CountryId)
	AND (@IsTwoFactorEnabled IS NULL OR U.TwoFactorEnabled = @IsTwoFactorEnabled)
	AND (@EmailConfirmed IS NULL OR U.EmailConfirmed = @EmailConfirmed)
	AND (@IsActive IS NULL OR U.IsActive = @IsActive)
	AND (@UserCategoryId IS NULL OR U.UserCategoryId = @UserCategoryId)
	 ORDER BY 
        CASE WHEN @FieldToOrder = 'CountryName' AND @DirectionOrder = 'ASC' THEN CO.Name END ASC,
        CASE WHEN @FieldToOrder = 'CountryName' AND @DirectionOrder = 'DESC' THEN CO.Name END DESC,
		CASE WHEN @FieldToOrder = 'PersonalEmail' AND @DirectionOrder = 'ASC' THEN CD.PersonalEmail END ASC,
        CASE WHEN @FieldToOrder = 'PersonalEmail' AND @DirectionOrder = 'DESC' THEN CD.PersonalEmail END DESC,
		CASE WHEN @FieldToOrder = 'ShirtSize' AND @DirectionOrder = 'ASC' THEN CD.ShirtSize END ASC,
        CASE WHEN @FieldToOrder = 'ShirtSize' AND @DirectionOrder = 'DESC' THEN CD.ShirtSize END DESC,
		CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN U.Name + ' ' + U.LastName END ASC,
        CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN U.Name + ' ' + U.LastName END DESC,
		CASE WHEN @FieldToOrder = 'InternalEmail' AND @DirectionOrder = 'ASC' THEN U.Email END ASC,
        CASE WHEN @FieldToOrder = 'InternalEmail' AND @DirectionOrder = 'DESC' THEN U.Email END DESC,
		CASE WHEN @FieldToOrder = 'TwoFactorEnabled' AND @DirectionOrder = 'ASC' THEN U.TwoFactorEnabled END ASC,
        CASE WHEN @FieldToOrder = 'TwoFactorEnabled' AND @DirectionOrder = 'DESC' THEN U.TwoFactorEnabled END DESC,
		CASE WHEN @FieldToOrder = 'EmailConfirmed' AND @DirectionOrder = 'ASC' THEN U.EmailConfirmed END ASC,
        CASE WHEN @FieldToOrder = 'EmailConfirmed' AND @DirectionOrder = 'DESC' THEN U.EmailConfirmed END DESC,
		CASE WHEN @FieldToOrder = 'IsActive' AND @DirectionOrder = 'ASC' THEN U.IsActive END ASC,
        CASE WHEN @FieldToOrder = 'IsActive' AND @DirectionOrder = 'DESC' THEN U.IsActive END DESC,
		CASE WHEN @FieldToOrder = 'UserCategoryName' AND @DirectionOrder = 'ASC' THEN UC.Name END ASC,
        CASE WHEN @FieldToOrder = 'UserCategoryName' AND @DirectionOrder = 'DESC' THEN UC.Name END DESC,
		CD.CreationDate DESC
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_DETAILS_GetAllConsultantsWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
