using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class UpdateSPGetClientById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CLIENT_GetClientById
            @ClientId INT
                       AS
                       BEGIN
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
                         ,CD.ConsultantId AS SuccessManagerId
                         ,U.Name + ' ' + U.LastName AS SuccessManager
	                     ,C.LatePaymentFee
                         ,C.AdditionalEmailsForNotifications
                         ,C.AllowSentLatePaymentNotifications
                         FROM CLIENT C 
                         LEFT JOIN CONSULTANT_DETAILS CD ON C.SuccessManager = CD.ConsultantId
                         LEFT JOIN Users U ON CD.UserId = U.Id
	                     WHERE C.ClientId = @ClientId
                       END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetClientById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE GetClientById
            @ClientId INT
                       AS
                       BEGIN
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
                         ,CD.ConsultantId AS SuccessManagerId
                         ,U.Name + ' ' + U.LastName AS SuccessManager
	                     ,C.LatePaymentFee
                         ,C.AdditionalEmailsForNotifications
                         ,C.AllowSentLatePaymentNotifications
                         FROM CLIENT C 
                         LEFT JOIN CONSULTANT_DETAILS CD ON C.SuccessManagerId = CD.UserId
                         LEFT JOIN Users U ON C.SuccessManagerId = U.Id
	                     WHERE C.ClientId = @ClientId
                       END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CLIENT_GetClientById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
