using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createOvertimeColumnsinClientsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LimitNumHoursForOverTime",
                table: "CLIENT",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverTimeAmount",
                table: "CLIENT",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LimitNumHoursForOverTime",
                table: "CLIENT");

            migrationBuilder.DropColumn(
                name: "OverTimeAmount",
                table: "CLIENT");
        }
    }
}
