using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260402153000_AddCncMaterialPcbFields")]
    public partial class AddCncMaterialPcbFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PcbMaterialType",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportedBoardThicknesses",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsDoubleSided",
                table: "CncMaterials",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsSingleSided",
                table: "CncMaterials",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportsSingleSided",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "SupportsDoubleSided",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "SupportedBoardThicknesses",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "PcbMaterialType",
                table: "CncMaterials");
        }
    }
}
