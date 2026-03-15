using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaterialColorId",
                table: "Print3dServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaterialColors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HexCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialColors_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Print3dServiceRequests_MaterialColorId",
                table: "Print3dServiceRequests",
                column: "MaterialColorId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialColors_MaterialId_SortOrder",
                table: "MaterialColors",
                columns: new[] { "MaterialId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Print3dServiceRequests_MaterialColors_MaterialColorId",
                table: "Print3dServiceRequests",
                column: "MaterialColorId",
                principalTable: "MaterialColors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Print3dServiceRequests_MaterialColors_MaterialColorId",
                table: "Print3dServiceRequests");

            migrationBuilder.DropTable(
                name: "MaterialColors");

            migrationBuilder.DropIndex(
                name: "IX_Print3dServiceRequests_MaterialColorId",
                table: "Print3dServiceRequests");

            migrationBuilder.DropColumn(
                name: "MaterialColorId",
                table: "Print3dServiceRequests");
        }
    }
}
