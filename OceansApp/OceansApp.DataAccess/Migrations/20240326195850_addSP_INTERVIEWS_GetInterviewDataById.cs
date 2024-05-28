using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_INTERVIEWS_GetInterviewDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_INTERVIEWS_GetInterviewDataById
            @InterviewId INT
            AS
            BEGIN
            SELECT I.InterviewId
            ,I.ConsultantId
	        ,Uc.Name + ' ' + Uc.LastName AS ConsultantName
	        ,Uc.Email AS ConsultantEmail
            ,I.DurationMinutes
            ,I.Date
            FROM INTERVIEWS I
            INNER JOIN CONSULTANT_DETAILS CD ON I.ConsultantId = CD.ConsultantId
            INNER JOIN Users Uc ON CD.UserId = Uc.Id
            WHERE I.InterviewId = @InterviewId;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_INTERVIEWS_GetInterviewDataById");
        }
    }
}
