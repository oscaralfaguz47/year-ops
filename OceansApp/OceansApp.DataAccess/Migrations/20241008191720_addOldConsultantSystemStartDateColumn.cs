using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addOldConsultantSystemStartDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OldConsultantSystemStartDate",
                table: "CONSULTANT_DETAILS",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_OldConsultantSystemStartDate",
                table: "CONSULTANT_DETAILS",
                column: "OldConsultantSystemStartDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_OldConsultantSystemStartDate",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "OldConsultantSystemStartDate",
                table: "CONSULTANT_DETAILS");
        }
    }
}
