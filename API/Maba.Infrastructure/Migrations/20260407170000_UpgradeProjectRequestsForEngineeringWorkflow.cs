using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260407170000_UpgradeProjectRequestsForEngineeringWorkflow")]
    public partial class UpgradeProjectRequestsForEngineeringWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentsJson",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainDomain",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectStage",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectType",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredCapabilitiesJson",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentsJson",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "MainDomain",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "ProjectStage",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "ProjectType",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "RequiredCapabilitiesJson",
                table: "ProjectRequests");
        }
    }
}
