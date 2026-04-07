using System;
using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260407190000_AddEngineeringWorkflowToProjectRequests")]
    public partial class AddEngineeringWorkflowToProjectRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add WorkflowStatus (nullable first so we can populate from existing Status)
            migrationBuilder.AddColumn<string>(
                name: "WorkflowStatus",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                nullable: true);

            // Migrate existing integer Status values → WorkflowStatus strings
            migrationBuilder.Sql(@"
                UPDATE ProjectRequests SET WorkflowStatus =
                    CASE Status
                        WHEN 0 THEN 'New'
                        WHEN 1 THEN 'UnderReview'
                        WHEN 2 THEN 'QuoteSent'
                        WHEN 3 THEN 'InExecution'
                        WHEN 4 THEN 'Completed'
                        ELSE 'New'
                    END
                WHERE WorkflowStatus IS NULL");

            // Assignment fields
            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "ProjectRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedToName",
                table: "ProjectRequests",
                type: "nvarchar(200)",
                nullable: true);

            // Internal engineering fields
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalFeasibility",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "ProjectRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedTimeline",
                table: "ProjectRequests",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplexityLevel",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);

            // Activity log table
            migrationBuilder.CreateTable(
                name: "ProjectRequestActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRequestActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRequestActivities_ProjectRequests_ProjectRequestId",
                        column: x => x.ProjectRequestId,
                        principalTable: "ProjectRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequestActivities_ProjectRequestId_CreatedAt",
                table: "ProjectRequestActivities",
                columns: new[] { "ProjectRequestId", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProjectRequestActivities");

            migrationBuilder.DropColumn(name: "WorkflowStatus", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "AssignedToUserId", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "AssignedToName", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "Priority", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "TechnicalFeasibility", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "EstimatedCost", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "EstimatedTimeline", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "ComplexityLevel", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "InternalNotes", table: "ProjectRequests");
        }
    }
}
