using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCncServiceRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "CncServiceRequests");

            migrationBuilder.RenameColumn(
                name: "WidthCm",
                table: "CncServiceRequests",
                newName: "WidthMm");

            migrationBuilder.RenameColumn(
                name: "HeightCm",
                table: "CncServiceRequests",
                newName: "HeightMm");

            migrationBuilder.RenameColumn(
                name: "CustomerPhone",
                table: "CncServiceRequests",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "CustomerNotes",
                table: "CncServiceRequests",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Operation",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PcbSide",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "Operation",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "PcbSide",
                table: "CncServiceRequests");

            migrationBuilder.RenameColumn(
                name: "WidthMm",
                table: "CncServiceRequests",
                newName: "WidthCm");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "CncServiceRequests",
                newName: "CustomerPhone");

            migrationBuilder.RenameColumn(
                name: "HeightMm",
                table: "CncServiceRequests",
                newName: "HeightCm");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CncServiceRequests",
                newName: "CustomerNotes");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
