using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createPaymentBookEntriesParentAndChildTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PAYMENT_BOOK_ENTRIES_PARENT",
                columns: table => new
                {
                    ParentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT_BOOK_ENTRIES_PARENT", x => x.ParentId);
                    table.ForeignKey(
                        name: "FK_PAYMENT_BOOK_ENTRIES_PARENT_COMPANIES_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANIES",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAYMENT_BOOK_ENTRIES_PARENT_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAYMENT_BOOK_ENTRIES_PARENT_Users_UserCreatedBy",
                        column: x => x.UserCreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT_BOOK_ENTRIES_CHILD",
                columns: table => new
                {
                    ChildId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    ConsultantPaymentId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT_BOOK_ENTRIES_CHILD", x => x.ChildId);
                    table.ForeignKey(
                        name: "FK_PAYMENT_BOOK_ENTRIES_CHILD_CONSULTANT_PAYMENTS_ConsultantPaymentId",
                        column: x => x.ConsultantPaymentId,
                        principalTable: "CONSULTANT_PAYMENTS",
                        principalColumn: "ConsultantPaymentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PAYMENT_BOOK_ENTRIES_CHILD_PAYMENT_BOOK_ENTRIES_PARENT_ParentId",
                        column: x => x.ParentId,
                        principalTable: "PAYMENT_BOOK_ENTRIES_PARENT",
                        principalColumn: "ParentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_CHILD_ChildId",
                table: "PAYMENT_BOOK_ENTRIES_CHILD",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_CHILD_ConsultantPaymentId",
                table: "PAYMENT_BOOK_ENTRIES_CHILD",
                column: "ConsultantPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_CHILD_ParentId",
                table: "PAYMENT_BOOK_ENTRIES_CHILD",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_PARENT_CompanyId",
                table: "PAYMENT_BOOK_ENTRIES_PARENT",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_PARENT_ParentId",
                table: "PAYMENT_BOOK_ENTRIES_PARENT",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_PARENT_TransactionStatusId",
                table: "PAYMENT_BOOK_ENTRIES_PARENT",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_PARENT_UserCreatedBy",
                table: "PAYMENT_BOOK_ENTRIES_PARENT",
                column: "UserCreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PAYMENT_BOOK_ENTRIES_CHILD");

            migrationBuilder.DropTable(
                name: "PAYMENT_BOOK_ENTRIES_PARENT");
        }
    }
}
