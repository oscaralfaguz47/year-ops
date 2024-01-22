using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateConsultantDetaiTableChangePrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_DETAILS",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.AddColumn<int>(
                name: "ConsultantId",
                table: "CONSULTANT_DETAILS",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<decimal>(
                name: "LatePaymentFee",
                table: "CLIENT",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_DETAILS",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_UserId",
                table: "CONSULTANT_DETAILS",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_DETAILS",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_UserId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropColumn(
                name: "ConsultantId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.AlterColumn<decimal>(
                name: "LatePaymentFee",
                table: "CLIENT",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_DETAILS",
                table: "CONSULTANT_DETAILS",
                column: "UserId");
        }
    }
}
