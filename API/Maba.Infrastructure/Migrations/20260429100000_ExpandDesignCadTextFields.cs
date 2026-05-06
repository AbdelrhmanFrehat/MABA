using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260429100000_ExpandDesignCadTextFields")]
    public partial class ExpandDesignCadTextFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Expand all free-text fields from their previous nvarchar limits to nvarchar(max)
            var cols = new[]
            {
                "Description", "IntendedUse", "MaterialNotes", "DimensionsNotes",
                "ToleranceNotes", "WhatNeedsChange", "CriticalSurfaces",
                "FitmentRequirements", "PurposeAndConstraints", "CustomerNotes", "AdminNotes"
            };

            foreach (var col in cols)
            {
                migrationBuilder.AlterColumn<string>(
                    name: col,
                    table: "DesignCadServiceRequests",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);
            }

            // Also expand Deadline
            migrationBuilder.AlterColumn<string>(
                name: "Deadline",
                table: "DesignCadServiceRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Deadline
            migrationBuilder.AlterColumn<string>(
                name: "Deadline",
                table: "DesignCadServiceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
