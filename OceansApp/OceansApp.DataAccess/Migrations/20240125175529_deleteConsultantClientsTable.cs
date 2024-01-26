using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class deleteConsultantClientsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_CLIENTS_CLIENT_ClientId",
                table: "CONSULTANT_CLIENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_ConsultantId",
                table: "CONSULTANT_CLIENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_CreatedBy",
                table: "CONSULTANT_CLIENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_SuccessManager",
                table: "CONSULTANT_CLIENTS");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_UpdatedBy",
                table: "CONSULTANT_CLIENTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_CLIENTS",
                table: "CONSULTANT_CLIENTS");

            migrationBuilder.RenameTable(
                name: "CONSULTANT_CLIENTS",
                newName: "ConsultantClient");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_CLIENTS_UpdatedBy",
                table: "ConsultantClient",
                newName: "IX_ConsultantClient_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_CLIENTS_SuccessManager",
                table: "ConsultantClient",
                newName: "IX_ConsultantClient_SuccessManager");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_CLIENTS_CreatedBy",
                table: "ConsultantClient",
                newName: "IX_ConsultantClient_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_CLIENTS_ClientId",
                table: "ConsultantClient",
                newName: "IX_ConsultantClient_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsultantClient",
                table: "ConsultantClient",
                columns: new[] { "ConsultantId", "ClientId", "SuccessManager" });

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantClient_CLIENT_ClientId",
                table: "ConsultantClient",
                column: "ClientId",
                principalTable: "CLIENT",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantClient_Users_ConsultantId",
                table: "ConsultantClient",
                column: "ConsultantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantClient_Users_CreatedBy",
                table: "ConsultantClient",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantClient_Users_SuccessManager",
                table: "ConsultantClient",
                column: "SuccessManager",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantClient_Users_UpdatedBy",
                table: "ConsultantClient",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantClient_CLIENT_ClientId",
                table: "ConsultantClient");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantClient_Users_ConsultantId",
                table: "ConsultantClient");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantClient_Users_CreatedBy",
                table: "ConsultantClient");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantClient_Users_SuccessManager",
                table: "ConsultantClient");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantClient_Users_UpdatedBy",
                table: "ConsultantClient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsultantClient",
                table: "ConsultantClient");

            migrationBuilder.RenameTable(
                name: "ConsultantClient",
                newName: "CONSULTANT_CLIENTS");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantClient_UpdatedBy",
                table: "CONSULTANT_CLIENTS",
                newName: "IX_CONSULTANT_CLIENTS_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantClient_SuccessManager",
                table: "CONSULTANT_CLIENTS",
                newName: "IX_CONSULTANT_CLIENTS_SuccessManager");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantClient_CreatedBy",
                table: "CONSULTANT_CLIENTS",
                newName: "IX_CONSULTANT_CLIENTS_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantClient_ClientId",
                table: "CONSULTANT_CLIENTS",
                newName: "IX_CONSULTANT_CLIENTS_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_CLIENTS",
                table: "CONSULTANT_CLIENTS",
                columns: new[] { "ConsultantId", "ClientId", "SuccessManager" });

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_CLIENTS_CLIENT_ClientId",
                table: "CONSULTANT_CLIENTS",
                column: "ClientId",
                principalTable: "CLIENT",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_ConsultantId",
                table: "CONSULTANT_CLIENTS",
                column: "ConsultantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_CreatedBy",
                table: "CONSULTANT_CLIENTS",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_SuccessManager",
                table: "CONSULTANT_CLIENTS",
                column: "SuccessManager",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_CLIENTS_Users_UpdatedBy",
                table: "CONSULTANT_CLIENTS",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
