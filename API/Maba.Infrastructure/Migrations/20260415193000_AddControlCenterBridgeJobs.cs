using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    public partial class AddControlCenterBridgeJobs : Migration
    {
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
                nullable: true);

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
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CcJobs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE CcJobs
                SET JobReference = COALESCE(JobReference, CONCAT('JOB-', CONVERT(nvarchar(36), Id))),
                    SourceType = COALESCE(SourceType, 'LEGACY'),
                    Title = COALESCE(Title, 'Legacy Control Center Job')
                WHERE JobReference IS NULL OR SourceType IS NULL OR Title IS NULL;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CcJobs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceType",
                table: "CcJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JobReference",
                table: "CcJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrgId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "CcJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
