using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenTN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedFieldChangeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PropertyID",
                table: "ChangeLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropertyID",
                table: "ChangeLogs");
        }
    }
}
