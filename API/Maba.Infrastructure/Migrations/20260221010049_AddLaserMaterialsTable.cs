using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLaserMaterialsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_Users_AssignedToUserId",
                table: "SupportConversations");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Orders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ILS",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Items",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ILS",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");

            migrationBuilder.CreateTable(
                name: "LaserMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinThicknessMm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxThicknessMm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NotesEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMetal = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaserMaterials", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_Users_AssignedToUserId",
                table: "SupportConversations",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_Users_AssignedToUserId",
                table: "SupportConversations");

            migrationBuilder.DropTable(
                name: "LaserMaterials");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Orders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ILS");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Items",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ILS");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_Users_AssignedToUserId",
                table: "SupportConversations",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
