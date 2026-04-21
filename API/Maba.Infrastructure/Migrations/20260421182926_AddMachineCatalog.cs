using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineCatalogCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IconKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineCatalogCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineCatalogFamilies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "MABA"),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineCatalogFamilies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineCatalogFamilies_MachineCatalogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MachineCatalogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MachineCatalogDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RevisionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayNameEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DisplayNameAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "MABA"),
                    TagsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeprecationNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuntimeBinding = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AxisConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Workspace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotionDefaults = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConnectionDefaults = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capabilities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSupport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Visualization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileRules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineCatalogDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineCatalogDefinitions_MachineCatalogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MachineCatalogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineCatalogDefinitions_MachineCatalogFamilies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "MachineCatalogFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineCatalogCategories_Code",
                table: "MachineCatalogCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineCatalogDefinitions_CategoryId",
                table: "MachineCatalogDefinitions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineCatalogDefinitions_Code",
                table: "MachineCatalogDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineCatalogDefinitions_FamilyId",
                table: "MachineCatalogDefinitions",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineCatalogDefinitions_IsActive_IsPublic_IsDeprecated",
                table: "MachineCatalogDefinitions",
                columns: new[] { "IsActive", "IsPublic", "IsDeprecated" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineCatalogFamilies_CategoryId",
                table: "MachineCatalogFamilies",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineCatalogFamilies_Code",
                table: "MachineCatalogFamilies",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineCatalogDefinitions");

            migrationBuilder.DropTable(
                name: "MachineCatalogFamilies");

            migrationBuilder.DropTable(
                name: "MachineCatalogCategories");
        }
    }
}
