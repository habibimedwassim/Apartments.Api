using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apartments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_EntityType",
                table: "ChangeLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_PropertyName",
                table: "ChangeLogs",
                column: "PropertyName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Role",
                table: "AspNetUsers",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChangeLogs_EntityType",
                table: "ChangeLogs");

            migrationBuilder.DropIndex(
                name: "IX_ChangeLogs_PropertyName",
                table: "ChangeLogs");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Role",
                table: "AspNetUsers");
        }
    }
}
