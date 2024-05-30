using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fourthUpdateSP_CLIENT_GetAllClientsWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CLIENT_GetAllClientsWithFilters
            @SearchText NVARCHAR(255),
    @StartDate DATE,
    @EndDate DATE,
    @IsActive NVARCHAR(1),
    @CompanyId NVARCHAR(8),
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
    FROM CLIENT C 
    LEFT JOIN CONSULTANT_DETAILS CD ON C.SuccessManager = CD.ConsultantId
	LEFT JOIN Users U ON CD.UserId = U.Id
    WHERE ((@SearchText IS NULL OR LOWER(C.Name) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(C.Contact) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Address) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Emails) LIKE '%' + LOWER(@SearchText) + '%'))
		AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (C.AdmissionDate >= @StartDate AND C.AdmissionDate <= @EndDate))
		AND (@IsActive IS NULL OR C.IsActive = @IsActive)
		AND (@CompanyId IS NULL OR C.CompanyId = @CompanyId)
		AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId)
		AND C.ClientCategory NOT LIKE '%CON%'
		AND C.ClientCategory NOT IN('ND')
		AND C.ClientCode NOT IN('OCE_C0028', 'OCE_C0029', 'OCE_C0030');

    -- Request with pagination
    SELECT
      C.ClientId
      ,C.Name
      ,C.Contact
      ,C.ContactOccupation
	  ,C.Emails
      ,C.AdmissionDate
      ,C.PaymentCondition
      ,C.IsActive
      ,C.ClientClass
      ,C.Address
      ,C.CompanyId
      ,U.Name + ' ' + U.LastName AS SuccessManager
	  ,C.LatePaymentFee
      ,C.AdditionalEmailsForNotifications
      ,C.AllowSentLatePaymentNotifications
      FROM CLIENT C 
      LEFT JOIN CONSULTANT_DETAILS CD ON C.SuccessManager = CD.ConsultantId
	  LEFT JOIN Users U ON CD.UserId = U.Id
      WHERE ((@SearchText IS NULL OR LOWER(C.Name) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(C.Contact) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Address) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Emails) LIKE '%' + LOWER(@SearchText) + '%'))
		AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (C.AdmissionDate >= @StartDate AND C.AdmissionDate <= @EndDate))
		AND (@IsActive IS NULL OR C.IsActive = @IsActive)
		AND (@CompanyId IS NULL OR C.CompanyId = @CompanyId)
		AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId)
		AND C.ClientCategory NOT LIKE '%CON%'
		AND C.ClientCategory NOT IN('ND')
		AND C.ClientCode NOT IN('OCE_C0028', 'OCE_C0029', 'OCE_C0030')
        ORDER BY 
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'ASC' THEN C.Name END ASC,
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'DESC' THEN C.Name END DESC,
        CASE WHEN @FieldToOrder = 'Contact' AND @DirectionOrder = 'ASC' THEN C.Contact END ASC,
        CASE WHEN @FieldToOrder = 'Contact' AND @DirectionOrder = 'DESC' THEN C.Contact END DESC,
        CASE WHEN @FieldToOrder = 'ContactOccupation' AND @DirectionOrder = 'DESC' THEN C.ContactOccupation END DESC,
        CASE WHEN @FieldToOrder = 'ContactOccupation' AND @DirectionOrder = 'ASC' THEN C.ContactOccupation END ASC,
		CASE WHEN @FieldToOrder = 'AdmissionDate' AND @DirectionOrder = 'DESC' THEN C.AdmissionDate END DESC,
        CASE WHEN @FieldToOrder = 'AdmissionDate' AND @DirectionOrder = 'ASC' THEN C.AdmissionDate END ASC,
		CASE WHEN @FieldToOrder = 'PaymentCondition' AND @DirectionOrder = 'DESC' THEN C.PaymentCondition END DESC,
        CASE WHEN @FieldToOrder = 'PaymentCondition' AND @DirectionOrder = 'ASC' THEN C.PaymentCondition END ASC,
		CASE WHEN @FieldToOrder = 'LatePaymentFee' AND @DirectionOrder = 'DESC' THEN C.LatePaymentFee END DESC,
        CASE WHEN @FieldToOrder = 'LatePaymentFee' AND @DirectionOrder = 'ASC' THEN C.LatePaymentFee END ASC,
        C.Name
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CLIENT_GetAllClientsWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CLIENT_GetAllClientsWithFilters
            @SearchText NVARCHAR(255),
    @StartDate DATE,
    @EndDate DATE,
    @IsActive NVARCHAR(1),
    @CompanyId NVARCHAR(8),
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
    FROM CLIENT C 
    LEFT JOIN CONSULTANT_DETAILS CD ON C.SuccessManager = CD.ConsultantId
	LEFT JOIN Users U ON CD.UserId = U.Id
    WHERE ((@SearchText IS NULL OR LOWER(C.Name) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(C.Contact) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Address) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Emails) LIKE '%' + LOWER(@SearchText) + '%'))
		AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (C.AdmissionDate >= @StartDate AND C.AdmissionDate <= @EndDate))
		AND (@IsActive IS NULL OR C.IsActive = @IsActive)
		AND (@CompanyId IS NULL OR C.CompanyId = @CompanyId)
		AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId)
		AND C.ClientCategory NOT LIKE '%CON%'
		AND C.ClientCategory NOT IN('ND')
		AND C.ClientCode NOT IN('OCE_C0028', 'OCE_C0029', 'OCE_C0030', 'OCE_C0040', 'OCEADMIN01', 'OCELL_C0001');

    -- Request with pagination
    SELECT
      C.ClientId
      ,C.Name
      ,C.Contact
      ,C.ContactOccupation
	  ,C.Emails
      ,C.AdmissionDate
      ,C.PaymentCondition
      ,C.IsActive
      ,C.ClientClass
      ,C.Address
      ,C.CompanyId
      ,U.Name + ' ' + U.LastName AS SuccessManager
	  ,C.LatePaymentFee
      ,C.AdditionalEmailsForNotifications
      ,C.AllowSentLatePaymentNotifications
      FROM CLIENT C 
      LEFT JOIN CONSULTANT_DETAILS CD ON C.SuccessManager = CD.ConsultantId
	  LEFT JOIN Users U ON CD.UserId = U.Id
      WHERE ((@SearchText IS NULL OR LOWER(C.Name) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(C.Contact) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Address) LIKE '%' + LOWER(@SearchText) + '%')
		OR (@SearchText IS NULL OR LOWER(C.Emails) LIKE '%' + LOWER(@SearchText) + '%'))
		AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (C.AdmissionDate >= @StartDate AND C.AdmissionDate <= @EndDate))
		AND (@IsActive IS NULL OR C.IsActive = @IsActive)
		AND (@CompanyId IS NULL OR C.CompanyId = @CompanyId)
		AND (@SuccessManagerId IS NULL OR CD.ConsultantId = @SuccessManagerId)
		AND C.ClientCategory NOT LIKE '%CON%'
		AND C.ClientCategory NOT IN('ND')
		AND C.ClientCode NOT IN('OCE_C0028', 'OCE_C0029', 'OCE_C0030', 'OCE_C0040', 'OCEADMIN01', 'OCELL_C0001')
        ORDER BY 
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'ASC' THEN C.Name END ASC,
        CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'DESC' THEN C.Name END DESC,
        CASE WHEN @FieldToOrder = 'Contact' AND @DirectionOrder = 'ASC' THEN C.Contact END ASC,
        CASE WHEN @FieldToOrder = 'Contact' AND @DirectionOrder = 'DESC' THEN C.Contact END DESC,
        CASE WHEN @FieldToOrder = 'ContactOccupation' AND @DirectionOrder = 'DESC' THEN C.ContactOccupation END DESC,
        CASE WHEN @FieldToOrder = 'ContactOccupation' AND @DirectionOrder = 'ASC' THEN C.ContactOccupation END ASC,
		CASE WHEN @FieldToOrder = 'AdmissionDate' AND @DirectionOrder = 'DESC' THEN C.AdmissionDate END DESC,
        CASE WHEN @FieldToOrder = 'AdmissionDate' AND @DirectionOrder = 'ASC' THEN C.AdmissionDate END ASC,
		CASE WHEN @FieldToOrder = 'PaymentCondition' AND @DirectionOrder = 'DESC' THEN C.PaymentCondition END DESC,
        CASE WHEN @FieldToOrder = 'PaymentCondition' AND @DirectionOrder = 'ASC' THEN C.PaymentCondition END ASC,
		CASE WHEN @FieldToOrder = 'LatePaymentFee' AND @DirectionOrder = 'DESC' THEN C.LatePaymentFee END DESC,
        CASE WHEN @FieldToOrder = 'LatePaymentFee' AND @DirectionOrder = 'ASC' THEN C.LatePaymentFee END ASC,
        C.Name
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CLIENT_GetAllClientsWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
