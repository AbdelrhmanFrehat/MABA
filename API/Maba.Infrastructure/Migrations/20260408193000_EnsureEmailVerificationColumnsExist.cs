using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260408193000_EnsureEmailVerificationColumnsExist")]
    public partial class EnsureEmailVerificationColumnsExist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'EmailVerificationToken') IS NULL
                BEGIN
                    ALTER TABLE [Users] ADD [EmailVerificationToken] nvarchar(500) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'EmailVerificationTokenExpiresAt') IS NULL
                BEGIN
                    ALTER TABLE [Users] ADD [EmailVerificationTokenExpiresAt] datetime2 NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'EmailVerificationToken') IS NOT NULL
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [EmailVerificationToken];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'EmailVerificationTokenExpiresAt') IS NOT NULL
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [EmailVerificationTokenExpiresAt];
                END
                """);
        }
    }
}
