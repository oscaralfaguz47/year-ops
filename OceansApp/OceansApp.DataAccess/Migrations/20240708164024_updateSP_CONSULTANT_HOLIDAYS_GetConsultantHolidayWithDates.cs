using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateSP_CONSULTANT_HOLIDAYS_GetConsultantHolidayWithDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_HOLIDAYS_GetConsultantHolidayWithDates
        @ConsultantHolidayId INT
        AS
        BEGIN
            SELECT ConsultantHolidayId, Name 
            FROM CONSULTANT_HOLIDAYS 
            WHERE ConsultantHolidayId = @ConsultantHolidayId;
            
            SELECT ConsultantHolidayDateId, Name, Date
            FROM CONSULTANT_HOLIDAY_DATES 
            WHERE ConsultantHolidayId = @ConsultantHolidayId;
        END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_GetAllClientsForSelect");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_GetAllClientsForSelect
                                AS
                                BEGIN
                                    SELECT 
                                     ClientId
                                     ,Name
                                     FROM CLIENT
		                             WHERE ClientCategory NOT LIKE '%CON%'
		                             AND ClientCategory NOT IN('ND')
		                             AND ClientCode NOT IN('OCE_C0028', 'OCE_C0029', 'OCE_C0030');
                                END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_HOLIDAYS_GetConsultantHolidayWithDates");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
