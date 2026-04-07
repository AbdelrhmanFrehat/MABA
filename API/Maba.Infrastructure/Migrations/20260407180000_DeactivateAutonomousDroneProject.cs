using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maba.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260407180000_DeactivateAutonomousDroneProject")]
    public partial class DeactivateAutonomousDroneProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Projects SET IsActive = 0 WHERE Slug = 'autonomous-inspection-drone'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Projects SET IsActive = 1 WHERE Slug = 'autonomous-inspection-drone'");
        }
    }
}
