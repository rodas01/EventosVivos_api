using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventosVivos_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToVenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ubicacion",
                table: "venues",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ubicacion",
                table: "venues");
        }
    }
}
