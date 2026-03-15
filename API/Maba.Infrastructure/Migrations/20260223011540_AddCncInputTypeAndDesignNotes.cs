using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCncInputTypeAndDesignNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DesignNotes",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngraveType",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoleCount",
                table: "CncServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HoleDiameterMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InputType",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PocketDepthMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesignNotes",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "EngraveType",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "HoleCount",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "HoleDiameterMm",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "InputType",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "PocketDepthMm",
                table: "CncServiceRequests");
        }
    }
}
