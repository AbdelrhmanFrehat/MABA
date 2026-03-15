using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignCadServiceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DesignCadServiceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TargetProcess = table.Column<int>(type: "int", nullable: true),
                    IntendedUse = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaterialNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DimensionsNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ToleranceNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WhatNeedsChange = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CriticalSurfaces = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FitmentRequirements = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PurposeAndConstraints = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Deadline = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasPhysicalPart = table.Column<bool>(type: "bit", nullable: false),
                    LegalConfirmation = table.Column<bool>(type: "bit", nullable: false),
                    CanDeliverPhysicalPart = table.Column<bool>(type: "bit", nullable: false),
                    CustomerNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    QuotedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignCadServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignCadServiceRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DesignServiceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IntendedUse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaterialPreference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DimensionsNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ToleranceLevel = table.Column<int>(type: "int", nullable: true),
                    BudgetRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Timeline = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpOwnershipConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    QuotedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeliveryNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuotedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignServiceRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DesignCadServiceRequestAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignCadServiceRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignCadServiceRequestAttachments_DesignCadServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "DesignCadServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignServiceRequestAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignServiceRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignServiceRequestAttachments_DesignServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "DesignServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignCadServiceRequestAttachments_RequestId",
                table: "DesignCadServiceRequestAttachments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignCadServiceRequests_CreatedAt",
                table: "DesignCadServiceRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DesignCadServiceRequests_ReferenceNumber",
                table: "DesignCadServiceRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignCadServiceRequests_RequestType",
                table: "DesignCadServiceRequests",
                column: "RequestType");

            migrationBuilder.CreateIndex(
                name: "IX_DesignCadServiceRequests_Status",
                table: "DesignCadServiceRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DesignCadServiceRequests_UserId",
                table: "DesignCadServiceRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignServiceRequestAttachments_RequestId",
                table: "DesignServiceRequestAttachments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignServiceRequests_CreatedAt",
                table: "DesignServiceRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DesignServiceRequests_ReferenceNumber",
                table: "DesignServiceRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignServiceRequests_RequestType",
                table: "DesignServiceRequests",
                column: "RequestType");

            migrationBuilder.CreateIndex(
                name: "IX_DesignServiceRequests_Status",
                table: "DesignServiceRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DesignServiceRequests_UserId",
                table: "DesignServiceRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesignCadServiceRequestAttachments");

            migrationBuilder.DropTable(
                name: "DesignServiceRequestAttachments");

            migrationBuilder.DropTable(
                name: "DesignCadServiceRequests");

            migrationBuilder.DropTable(
                name: "DesignServiceRequests");
        }
    }
}
