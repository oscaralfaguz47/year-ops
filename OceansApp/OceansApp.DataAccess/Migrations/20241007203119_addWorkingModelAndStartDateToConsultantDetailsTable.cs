using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addWorkingModelAndStartDateToConsultantDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "CONSULTANT_DETAILS",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "WorkingModel",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_StartDate",
                table: "CONSULTANT_DETAILS",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_WorkingModel",
                table: "CONSULTANT_DETAILS",
                column: "WorkingModel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_StartDate",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_WorkingModel",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "WorkingModel",
                table: "CONSULTANT_DETAILS");
        }
    }
}
