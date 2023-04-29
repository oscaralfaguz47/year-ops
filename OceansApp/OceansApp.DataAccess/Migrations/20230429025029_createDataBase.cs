using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class createDataBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACCOUNTING_ACCOUNT",
                columns: table => new
                {
                    IdAccountingAccount = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AccountingAccountType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    DetailedType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Balance = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    AcceptData = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    UseCostCenter = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    UseThird = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateHour = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNTING_ACCOUNT", x => x.IdAccountingAccount);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    DeactivationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                columns: table => new
                {
                    IdAccountingAccount = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE", x => x.IdAccountingAccount);
                });

            migrationBuilder.CreateTable(
                name: "CALCULATOR_GLOBAL_CONFIGURATIONS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeopleNumber = table.Column<int>(type: "int", nullable: false),
                    NumLaborDaysInMonth = table.Column<double>(type: "float", nullable: false),
                    AdditionalGlobalIncrease = table.Column<double>(type: "float", nullable: false),
                    ProfitGreenClientAAA = table.Column<double>(type: "float", nullable: false),
                    ProfitGreenClientAA = table.Column<double>(type: "float", nullable: false),
                    ProfitGreenPartner = table.Column<double>(type: "float", nullable: false),
                    ProfitYellowClientAAA = table.Column<double>(type: "float", nullable: false),
                    ProfitYellowClientAA = table.Column<double>(type: "float", nullable: false),
                    ProfitYellowPartner = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALCULATOR_GLOBAL_CONFIGURATIONS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CLIENT",
                columns: table => new
                {
                    IdClient = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Contact = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactOccupation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Phone1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentCondition = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    ClientCategory = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ClientClass = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Emails = table.Column<string>(type: "nvarchar(249)", maxLength: 249, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENT", x => x.IdClient);
                });

            migrationBuilder.CreateTable(
                name: "COST_CENTER",
                columns: table => new
                {
                    IdCostCenter = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AcceptData = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COST_CENTER", x => x.IdCostCenter);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CALCULATOR_SEARCH_HISTORY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SearchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SearchByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALCULATOR_SEARCH_HISTORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CALCULATOR_SEARCH_HISTORY_AspNetUsers_SearchByUserId",
                        column: x => x.SearchByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DATA_UPDATE_DATES",
                columns: table => new
                {
                    IdUpdateDate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SectionsUpdated = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DATA_UPDATE_DATES", x => x.IdUpdateDate);
                    table.ForeignKey(
                        name: "FK_DATA_UPDATE_DATES_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                columns: table => new
                {
                    IdCostCenter = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Increase = table.Column<double>(type: "float", nullable: true),
                    IdUserUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DateLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS", x => x.IdCostCenter);
                    table.ForeignKey(
                        name: "FK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_AspNetUsers_IdUserUpdatedBy",
                        column: x => x.IdUserUpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_COST_CENTER_IdCostCenter",
                        column: x => x.IdCostCenter,
                        principalTable: "COST_CENTER",
                        principalColumn: "IdCostCenter",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LEDGER_MOVEMENT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSeat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Consecutive = table.Column<int>(type: "int", nullable: false),
                    IdCostCenter = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    IdAccountingAccount = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocalDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LocalCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountingType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEDGER_MOVEMENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LEDGER_MOVEMENT_ACCOUNTING_ACCOUNT_IdAccountingAccount",
                        column: x => x.IdAccountingAccount,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "IdAccountingAccount",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LEDGER_MOVEMENT_COST_CENTER_IdCostCenter",
                        column: x => x.IdCostCenter,
                        principalTable: "COST_CENTER",
                        principalColumn: "IdCostCenter",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS_IdUserUpdatedBy",
                table: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS",
                column: "IdUserUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CALCULATOR_SEARCH_HISTORY_SearchByUserId",
                table: "CALCULATOR_SEARCH_HISTORY",
                column: "SearchByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DATA_UPDATE_DATES_CreatedBy",
                table: "DATA_UPDATE_DATES",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_IdAccountingAccount",
                table: "LEDGER_MOVEMENT",
                column: "IdAccountingAccount");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_IdCostCenter",
                table: "LEDGER_MOVEMENT",
                column: "IdCostCenter");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE");

            migrationBuilder.DropTable(
                name: "CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS");

            migrationBuilder.DropTable(
                name: "CALCULATOR_GLOBAL_CONFIGURATIONS");

            migrationBuilder.DropTable(
                name: "CALCULATOR_SEARCH_HISTORY");

            migrationBuilder.DropTable(
                name: "CLIENT");

            migrationBuilder.DropTable(
                name: "DATA_UPDATE_DATES");

            migrationBuilder.DropTable(
                name: "LEDGER_MOVEMENT");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropTable(
                name: "COST_CENTER");
        }
    }
}
