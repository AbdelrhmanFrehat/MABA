using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewIsRejected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "Reviews");
        }
    }
}
