using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260409210000_AddQuotationsAndCommercialLinks")]
    public partial class AddQuotationsAndCommercialLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Quotations table ──────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceRequestType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SourceRequestReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    QuotationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "ILS"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConvertedToOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Quotations_CustomerId", table: "Quotations", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_Quotations_SourceRequestId", table: "Quotations", column: "SourceRequestId");
            migrationBuilder.CreateIndex(name: "IX_Quotations_Status", table: "Quotations", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Quotations_QuotationNumber", table: "Quotations", column: "QuotationNumber", unique: true);

            // ── QuotationItems table ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "QuotationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pcs"),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationItems_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_QuotationItems_QuotationId", table: "QuotationItems", column: "QuotationId");

            // ── Orders: add commercial pipeline columns ────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceQuotationId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceRequestId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceRequestType",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceRequestReference",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_Orders_CustomerId", table: "Orders", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_Orders_SourceQuotationId", table: "Orders", column: "SourceQuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Quotations_SourceQuotationId",
                table: "Orders",
                column: "SourceQuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── OrderItems: add Description + fix Quantity type ───────────────
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "OrderItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // ── ProjectRequests: LinkedQuotationId ────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedQuotationId",
                table: "ProjectRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_ProjectRequests_LinkedQuotationId", table: "ProjectRequests", column: "LinkedQuotationId");

            // ── CncServiceRequests: LinkedQuotationId ─────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedQuotationId",
                table: "CncServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_CncServiceRequests_LinkedQuotationId", table: "CncServiceRequests", column: "LinkedQuotationId");

            // ── LaserServiceRequests: LinkedQuotationId ───────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedQuotationId",
                table: "LaserServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_LaserServiceRequests_LinkedQuotationId", table: "LaserServiceRequests", column: "LinkedQuotationId");

            // ── Print3dServiceRequests: LinkedQuotationId ─────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedQuotationId",
                table: "Print3dServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_Print3dServiceRequests_LinkedQuotationId", table: "Print3dServiceRequests", column: "LinkedQuotationId");

            // ── DesignServiceRequests: CustomerId + LinkedQuotationId ──────────
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DesignServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LinkedQuotationId",
                table: "DesignServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_DesignServiceRequests_CustomerId", table: "DesignServiceRequests", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_DesignServiceRequests_LinkedQuotationId", table: "DesignServiceRequests", column: "LinkedQuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_DesignServiceRequests_Customers_CustomerId",
                table: "DesignServiceRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── DesignCadServiceRequests: CustomerId + LinkedQuotationId ───────
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DesignCadServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LinkedQuotationId",
                table: "DesignCadServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_DesignCadServiceRequests_CustomerId", table: "DesignCadServiceRequests", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_DesignCadServiceRequests_LinkedQuotationId", table: "DesignCadServiceRequests", column: "LinkedQuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_DesignCadServiceRequests_Customers_CustomerId",
                table: "DesignCadServiceRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove FKs first
            migrationBuilder.DropForeignKey(name: "FK_Orders_Customers_CustomerId", table: "Orders");
            migrationBuilder.DropForeignKey(name: "FK_Orders_Quotations_SourceQuotationId", table: "Orders");
            migrationBuilder.DropForeignKey(name: "FK_DesignServiceRequests_Customers_CustomerId", table: "DesignServiceRequests");
            migrationBuilder.DropForeignKey(name: "FK_DesignCadServiceRequests_Customers_CustomerId", table: "DesignCadServiceRequests");

            // Drop indexes
            migrationBuilder.DropIndex(name: "IX_Orders_CustomerId", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_SourceQuotationId", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_ProjectRequests_LinkedQuotationId", table: "ProjectRequests");
            migrationBuilder.DropIndex(name: "IX_CncServiceRequests_LinkedQuotationId", table: "CncServiceRequests");
            migrationBuilder.DropIndex(name: "IX_LaserServiceRequests_LinkedQuotationId", table: "LaserServiceRequests");
            migrationBuilder.DropIndex(name: "IX_Print3dServiceRequests_LinkedQuotationId", table: "Print3dServiceRequests");
            migrationBuilder.DropIndex(name: "IX_DesignServiceRequests_CustomerId", table: "DesignServiceRequests");
            migrationBuilder.DropIndex(name: "IX_DesignServiceRequests_LinkedQuotationId", table: "DesignServiceRequests");
            migrationBuilder.DropIndex(name: "IX_DesignCadServiceRequests_CustomerId", table: "DesignCadServiceRequests");
            migrationBuilder.DropIndex(name: "IX_DesignCadServiceRequests_LinkedQuotationId", table: "DesignCadServiceRequests");

            // Drop columns
            migrationBuilder.DropColumn(name: "CustomerId", table: "Orders");
            migrationBuilder.DropColumn(name: "SourceQuotationId", table: "Orders");
            migrationBuilder.DropColumn(name: "SourceRequestId", table: "Orders");
            migrationBuilder.DropColumn(name: "SourceRequestType", table: "Orders");
            migrationBuilder.DropColumn(name: "SourceRequestReference", table: "Orders");
            migrationBuilder.DropColumn(name: "Description", table: "OrderItems");
            migrationBuilder.DropColumn(name: "LinkedQuotationId", table: "ProjectRequests");
            migrationBuilder.DropColumn(name: "LinkedQuotationId", table: "CncServiceRequests");
            migrationBuilder.DropColumn(name: "LinkedQuotationId", table: "LaserServiceRequests");
            migrationBuilder.DropColumn(name: "LinkedQuotationId", table: "Print3dServiceRequests");
            migrationBuilder.DropColumn(name: "CustomerId", table: "DesignServiceRequests");
            migrationBuilder.DropColumn(name: "LinkedQuotationId", table: "DesignServiceRequests");
            migrationBuilder.DropColumn(name: "CustomerId", table: "DesignCadServiceRequests");
            migrationBuilder.DropColumn(name: "LinkedQuotationId", table: "DesignCadServiceRequests");

            // Drop tables (QuotationItems first due to FK)
            migrationBuilder.DropTable(name: "QuotationItems");
            migrationBuilder.DropTable(name: "Quotations");
        }
    }
}
