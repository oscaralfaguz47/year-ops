using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateAccountPayableMovementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MovementTypeName",
                table: "ACCOUNTS_PAYABLE_MOVEMENTS",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ACCOUNTS_PAYABLE_MOVEMENTS",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ACCOUNTS_PAYABLE_MOVEMENTS");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ACCOUNTS_PAYABLE_MOVEMENTS",
                newName: "MovementTypeName");
        }
    }
}
