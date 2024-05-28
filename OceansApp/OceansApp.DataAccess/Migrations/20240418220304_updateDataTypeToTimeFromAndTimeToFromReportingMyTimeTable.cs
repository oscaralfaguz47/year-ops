using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateDataTypeToTimeFromAndTimeToFromReportingMyTimeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TimeTo",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeFrom",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TimeTo",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "time",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TimeFrom",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "time",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5,
                oldNullable: true);
        }
    }
}
