using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "CncServiceRequests",
                newName: "ProjectDescription");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CncServiceRequests",
                newName: "PcbSide");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CncServiceRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "CncServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DepthMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepthMode",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignNotes",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignSourceType",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedPrice",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HeightMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationType",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcbMaterial",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcbOperation",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PcbThickness",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CncServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "CncServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceMode",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ThicknessMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RepliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "DepthMm",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "DepthMode",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "DesignNotes",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "DesignSourceType",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedPrice",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "HeightMm",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "OperationType",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "PcbMaterial",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "PcbOperation",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "PcbThickness",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceMode",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "ThicknessMm",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "WidthMm",
                table: "CncServiceRequests");

            migrationBuilder.RenameColumn(
                name: "ProjectDescription",
                table: "CncServiceRequests",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "PcbSide",
                table: "CncServiceRequests",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
