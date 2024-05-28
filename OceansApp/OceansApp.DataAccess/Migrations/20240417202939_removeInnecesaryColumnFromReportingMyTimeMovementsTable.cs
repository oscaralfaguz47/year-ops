using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removeInnecesaryColumnFromReportingMyTimeMovementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "REPORTING_MY_TIME_MOVEMENTS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
