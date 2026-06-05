using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEffectiveDateToAdminPtoConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Defaults existing rows (including the already-seeded production config) to the
            // admin PTO go-live date so accrual and usage are bounded from that point forward.
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "ADMIN_PTO_CONFIGURATION",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "ADMIN_PTO_CONFIGURATION");
        }
    }
}
