using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <summary>
    /// Adds CustomerId and UserId to all service request tables so that website
    /// user submissions automatically create (or reuse) a CRM Customer record.
    /// </summary>
    public partial class AddCustomerIdToServiceRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── CncServiceRequests ───────────────────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "CncServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "CncServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CncServiceRequests_UserId",
                table: "CncServiceRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CncServiceRequests_CustomerId",
                table: "CncServiceRequests",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CncServiceRequests_Users_UserId",
                table: "CncServiceRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CncServiceRequests_Customers_CustomerId",
                table: "CncServiceRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── LaserServiceRequests ─────────────────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "LaserServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "LaserServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaserServiceRequests_UserId",
                table: "LaserServiceRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LaserServiceRequests_CustomerId",
                table: "LaserServiceRequests",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_LaserServiceRequests_Users_UserId",
                table: "LaserServiceRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LaserServiceRequests_Customers_CustomerId",
                table: "LaserServiceRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── ProjectRequests ──────────────────────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ProjectRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "ProjectRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_UserId",
                table: "ProjectRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_CustomerId",
                table: "ProjectRequests",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Users_UserId",
                table: "ProjectRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Customers_CustomerId",
                table: "ProjectRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── Print3dServiceRequests ───────────────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Print3dServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Print3dServiceRequests_CustomerId",
                table: "Print3dServiceRequests",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Print3dServiceRequests_Customers_CustomerId",
                table: "Print3dServiceRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_CncServiceRequests_Users_UserId", table: "CncServiceRequests");
            migrationBuilder.DropForeignKey(name: "FK_CncServiceRequests_Customers_CustomerId", table: "CncServiceRequests");
            migrationBuilder.DropIndex(name: "IX_CncServiceRequests_UserId", table: "CncServiceRequests");
            migrationBuilder.DropIndex(name: "IX_CncServiceRequests_CustomerId", table: "CncServiceRequests");
            migrationBuilder.DropColumn(name: "UserId", table: "CncServiceRequests");
            migrationBuilder.DropColumn(name: "CustomerId", table: "CncServiceRequests");

            migrationBuilder.DropForeignKey(name: "FK_LaserServiceRequests_Users_UserId", table: "LaserServiceRequests");
            migrationBuilder.DropForeignKey(name: "FK_LaserServiceRequests_Customers_CustomerId", table: "LaserServiceRequests");
            migrationBuilder.DropIndex(name: "IX_LaserServiceRequests_UserId", table: "LaserServiceRequests");
            migrationBuilder.DropIndex(name: "IX_LaserServiceRequests_CustomerId", table: "LaserServiceRequests");
            migrationBuilder.DropColumn(name: "UserId", table: "LaserServiceRequests");
            migrationBuilder.DropColumn(name: "CustomerId", table: "LaserServiceRequests");

            migrationBuilder.DropForeignKey(name: "FK_ProjectRequests_Users_UserId", table: "ProjectRequests");
            migrationBuilder.DropForeignKey(name: "FK_ProjectRequests_Customers_CustomerId", table: "ProjectRequests");
            migrationBuilder.DropIndex(name: "IX_ProjectRequests_UserId", table: "ProjectRequests");
            migrationBuilder.DropIndex(name: "IX_ProjectRequests_CustomerId", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "UserId", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "CustomerId", table: "ProjectRequests");

            migrationBuilder.DropForeignKey(name: "FK_Print3dServiceRequests_Customers_CustomerId", table: "Print3dServiceRequests");
            migrationBuilder.DropIndex(name: "IX_Print3dServiceRequests_CustomerId", table: "Print3dServiceRequests");
            migrationBuilder.DropColumn(name: "CustomerId", table: "Print3dServiceRequests");
        }
    }
}
