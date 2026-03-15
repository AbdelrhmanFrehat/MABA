using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCncMaterialRulesEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CncServiceRequests_CncMaterials_MaterialId",
                table: "CncServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequests_Projects_ProjectId",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Category",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_IsActive_SortOrder",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Slug",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Status",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_CreatedAt",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_ReferenceNumber",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_Status",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "BudgetRange",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "Timeline",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "HeightMm",
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
                name: "Operation",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "PcbSide",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "PocketDepthMm",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "QuotedPrice",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "ThicknessMm",
                table: "CncServiceRequests");

            migrationBuilder.DropColumn(
                name: "WidthMm",
                table: "CncServiceRequests");

            migrationBuilder.RenameColumn(
                name: "AdminNotes",
                table: "ProjectRequests",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "ServiceMode",
                table: "CncServiceRequests",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "CncServiceRequests",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "EngraveType",
                table: "CncServiceRequests",
                newName: "CustomerPhone");

            migrationBuilder.RenameColumn(
                name: "DesignNotes",
                table: "CncServiceRequests",
                newName: "CustomerEmail");

            migrationBuilder.AlterColumn<string>(
                name: "TitleEn",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "TitleAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "TechStackJson",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SummaryEn",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "SummaryAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Projects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFeatured",
                table: "Projects",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Projects",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "HighlightsJson",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GalleryJson",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionEn",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "MaterialId",
                table: "CncServiceRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "AllowCut",
                table: "CncMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowDrill",
                table: "CncMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowEngrave",
                table: "CncMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPocket",
                table: "CncMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CutNotesAr",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CutNotesEn",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrillNotesAr",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrillNotesEn",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngraveNotesAr",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngraveNotesEn",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPcbOnly",
                table: "CncMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCutDepthMm",
                table: "CncMaterials",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDrillDepthMm",
                table: "CncMaterials",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxEngraveDepthMm",
                table: "CncMaterials",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPocketDepthMm",
                table: "CncMaterials",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PocketNotesAr",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PocketNotesEn",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CncServiceRequests_CncMaterials_MaterialId",
                table: "CncServiceRequests",
                column: "MaterialId",
                principalTable: "CncMaterials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Projects_ProjectId",
                table: "ProjectRequests",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CncServiceRequests_CncMaterials_MaterialId",
                table: "CncServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequests_Projects_ProjectId",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "AllowCut",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "AllowDrill",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "AllowEngrave",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "AllowPocket",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "CutNotesAr",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "CutNotesEn",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "DrillNotesAr",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "DrillNotesEn",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "EngraveNotesAr",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "EngraveNotesEn",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "IsPcbOnly",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "MaxCutDepthMm",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "MaxDrillDepthMm",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "MaxEngraveDepthMm",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "MaxPocketDepthMm",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "PocketNotesAr",
                table: "CncMaterials");

            migrationBuilder.DropColumn(
                name: "PocketNotesEn",
                table: "CncMaterials");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "ProjectRequests",
                newName: "AdminNotes");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "CncServiceRequests",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "CustomerPhone",
                table: "CncServiceRequests",
                newName: "EngraveType");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "CncServiceRequests",
                newName: "ServiceMode");

            migrationBuilder.RenameColumn(
                name: "CustomerEmail",
                table: "CncServiceRequests",
                newName: "DesignNotes");

            migrationBuilder.AlterColumn<string>(
                name: "TitleEn",
                table: "Projects",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TitleAr",
                table: "Projects",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TechStackJson",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SummaryEn",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SummaryAr",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsFeatured",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "HighlightsJson",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GalleryJson",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionEn",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ProjectRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "ProjectRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BudgetRange",
                table: "ProjectRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "ProjectRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ProjectRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "ProjectRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                table: "ProjectRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Timeline",
                table: "ProjectRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CncServiceRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "MaterialId",
                table: "CncServiceRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
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

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "CncServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
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

            migrationBuilder.AddColumn<decimal>(
                name: "HeightMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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

            migrationBuilder.AddColumn<decimal>(
                name: "PocketDepthMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CncServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "QuotedPrice",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

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

            migrationBuilder.AddColumn<decimal>(
                name: "ThicknessMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthMm",
                table: "CncServiceRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "CncMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Category",
                table: "Projects",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsActive_SortOrder",
                table: "Projects",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Slug",
                table: "Projects",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_CreatedAt",
                table: "ProjectRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_ReferenceNumber",
                table: "ProjectRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_Status",
                table: "ProjectRequests",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_CncServiceRequests_CncMaterials_MaterialId",
                table: "CncServiceRequests",
                column: "MaterialId",
                principalTable: "CncMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Projects_ProjectId",
                table: "ProjectRequests",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
