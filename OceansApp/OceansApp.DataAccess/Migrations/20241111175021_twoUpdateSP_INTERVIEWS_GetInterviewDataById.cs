using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twoUpdateSP_INTERVIEWS_GetInterviewDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_INTERVIEWS_GetInterviewDataById
            @InterviewId INT
            AS
            BEGIN
            SET NOCOUNT ON;
        
            SELECT 
                I.InterviewId,
                I.ConsultantId,
                CONCAT(Uc.Name, ' ', Uc.LastName) AS ConsultantName,
                Uc.Email AS ConsultantEmail,
                I.DurationMinutes,
                I.Date,
                I.Detail
            FROM 
                INTERVIEWS AS I
            INNER JOIN 
                CONSULTANT_DETAILS AS CD ON I.ConsultantId = CD.ConsultantId
            INNER JOIN 
                Users AS Uc ON CD.UserId = Uc.Id
            WHERE 
                I.InterviewId = @InterviewId;
            END;";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_INTERVIEWS_GetInterviewDataById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_INTERVIEWS_GetInterviewDataById
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
            END;";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_INTERVIEWS_GetInterviewDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
