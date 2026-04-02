using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260402140000_AddSupportConversationSubjectAndLinks")]
    public partial class AddSupportConversationSubjectAndLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "SupportConversations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Support");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedOrderId",
                table: "SupportConversations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedDesignId",
                table: "SupportConversations",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RelatedDesignId", table: "SupportConversations");
            migrationBuilder.DropColumn(name: "RelatedOrderId", table: "SupportConversations");
            migrationBuilder.DropColumn(name: "Subject", table: "SupportConversations");
        }
    }
}
