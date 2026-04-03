using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrint3dRequestUsedSpoolAndEstimatedGrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedFilamentGrams",
                table: "Print3dServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UsedSpoolId",
                table: "Print3dServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Print3dServiceRequests_UsedSpoolId",
                table: "Print3dServiceRequests",
                column: "UsedSpoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Print3dServiceRequests_FilamentSpools_UsedSpoolId",
                table: "Print3dServiceRequests",
                column: "UsedSpoolId",
                principalTable: "FilamentSpools",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Print3dServiceRequests_FilamentSpools_UsedSpoolId",
                table: "Print3dServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Print3dServiceRequests_UsedSpoolId",
                table: "Print3dServiceRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedFilamentGrams",
                table: "Print3dServiceRequests");

            migrationBuilder.DropColumn(
                name: "UsedSpoolId",
                table: "Print3dServiceRequests");
        }
    }
}
