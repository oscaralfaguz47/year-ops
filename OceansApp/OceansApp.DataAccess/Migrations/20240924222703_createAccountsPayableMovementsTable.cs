using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createAccountsPayableMovementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACCOUNTS_PAYABLE_MOVEMENTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    MovementTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MovementTypeId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountPayableId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNTS_PAYABLE_MOVEMENTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ACCOUNTS_PAYABLE_MOVEMENTS_ACCOUNTS_PAYABLE_AccountPayableId",
                        column: x => x.AccountPayableId,
                        principalTable: "ACCOUNTS_PAYABLE",
                        principalColumn: "AccountPayableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ACCOUNTS_PAYABLE_MOVEMENTS_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_ACCOUNTS_PAYABLE_MOVEMENTS_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId",
                        column: x => x.MovementTypeId,
                        principalTable: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                        principalColumn: "MovementTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_MOVEMENTS_AccountPayableId",
                table: "ACCOUNTS_PAYABLE_MOVEMENTS",
                column: "AccountPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_MOVEMENTS_MovementTypeId",
                table: "ACCOUNTS_PAYABLE_MOVEMENTS",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_MOVEMENTS_ProjectId",
                table: "ACCOUNTS_PAYABLE_MOVEMENTS",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACCOUNTS_PAYABLE_MOVEMENTS");
        }
    }
}
