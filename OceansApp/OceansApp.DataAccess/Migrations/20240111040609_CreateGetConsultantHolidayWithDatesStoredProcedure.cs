using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class CreateGetConsultantHolidayWithDatesStoredProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE GetConsultantHolidayWithDates
                       @ConsultantHolidayId INT
                       AS
                       BEGIN
                       SELECT * FROM CONSULTANT_HOLIDAYS WHERE ConsultantHolidayId = @ConsultantHolidayId

                       SELECT ConsultantHolidayDateId, Name, Date
                       FROM CONSULTANT_HOLIDAY_DATES 
                       WHERE ConsultantHolidayId = @ConsultantHolidayId
                       END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetConsultantHolidayWithDates");
        }
    }
}
