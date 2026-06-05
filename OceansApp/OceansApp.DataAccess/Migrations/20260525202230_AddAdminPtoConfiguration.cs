using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPtoConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InitialAdminPtoBalance",
                table: "CONSULTANT_DETAILS",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ADMIN_PTO_CONFIGURATION",
                columns: table => new
                {
                    AdminPtoConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnualPaidDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADMIN_PTO_CONFIGURATION", x => x.AdminPtoConfigurationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADMIN_PTO_CONFIGURATION");

            migrationBuilder.DropColumn(
                name: "InitialAdminPtoBalance",
                table: "CONSULTANT_DETAILS");
        }
    }
}
