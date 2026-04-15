using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadableFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrgId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentsJson",
                table: "CcJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "CcJobs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CcJobs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobReference",
                table: "CcJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MachineType",
                table: "CcJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayloadJson",
                table: "CcJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "CcJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceReference",
                table: "CcJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "CcJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CcJobs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DownloadableFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DownloadCount = table.Column<int>(type: "int", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadableFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CcJobs_JobReference",
                table: "CcJobs",
                column: "JobReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CcJobs_MachineType_Status",
                table: "CcJobs",
                columns: new[] { "MachineType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CcJobs_SourceType_SourceId",
                table: "CcJobs",
                columns: new[] { "SourceType", "SourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadableFiles");

            migrationBuilder.DropIndex(
                name: "IX_CcJobs_JobReference",
                table: "CcJobs");

            migrationBuilder.DropIndex(
                name: "IX_CcJobs_MachineType_Status",
                table: "CcJobs");

            migrationBuilder.DropIndex(
                name: "IX_CcJobs_SourceType_SourceId",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "AttachmentsJson",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "JobReference",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "MachineType",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "PayloadJson",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "SourceReference",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "CcJobs");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CcJobs");

            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrgId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
