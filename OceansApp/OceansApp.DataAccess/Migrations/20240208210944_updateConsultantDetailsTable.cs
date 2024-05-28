using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateConsultantDetailsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "CONSULTANT_DETAILS",
                newName: "CreationDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "CONSULTANT_DETAILS",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCreatedBy",
                table: "CONSULTANT_DETAILS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserLastUpdatedBy",
                table: "CONSULTANT_DETAILS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_UserCreatedBy",
                table: "CONSULTANT_DETAILS",
                column: "UserCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_UserLastUpdatedBy",
                table: "CONSULTANT_DETAILS",
                column: "UserLastUpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_DETAILS_Users_UserCreatedBy",
                table: "CONSULTANT_DETAILS",
                column: "UserCreatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_DETAILS_Users_UserLastUpdatedBy",
                table: "CONSULTANT_DETAILS",
                column: "UserLastUpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_Users_UserCreatedBy",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_DETAILS_Users_UserLastUpdatedBy",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_UserCreatedBy",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_UserLastUpdatedBy",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "UserCreatedBy",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "UserLastUpdatedBy",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "CONSULTANT_DETAILS",
                newName: "StartDate");
        }
    }
}
