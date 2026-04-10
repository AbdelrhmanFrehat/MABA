using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDefaultCurrencyToILS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Change the DB default for Expenses.Currency from USD → ILS.
            // All other schema changes from this diff were already applied by
            // 20260409000000_AddCustomerIdToServiceRequests and
            // 20260409210000_AddQuotationsAndCommercialLinks; this migration only
            // carries the column-default fix so it is safe to run on any DB that
            // already has those two migrations applied.
            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ILS",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ILS");
        }
    }
}
