using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addStoredProcedureToGeClientById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE GetClientById
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
                         ,U.Name + ' ' + U.LastName AS SuccessManager
	                     ,C.LatePaymentFee
                         ,C.AdditionalEmailsForNotifications
                         ,C.AllowSentLatePaymentNotifications
                         FROM CLIENT C 
                         LEFT JOIN Users U ON C.SuccessManagerId = U.Id
	                     WHERE C.ClientId = @ClientId
                       END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetClientById");
        }
    }
}
