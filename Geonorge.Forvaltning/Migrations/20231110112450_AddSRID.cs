using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddSRID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "srid",
                table: "ForvaltningsObjektMetadata",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "srid",
                table: "ForvaltningsObjektMetadata");
        }
    }
}
