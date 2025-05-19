using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PropertyOrder",
                table: "ForvaltningsObjektPropertiesMetadata",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropertyOrder",
                table: "ForvaltningsObjektPropertiesMetadata");
        }
    }
}
