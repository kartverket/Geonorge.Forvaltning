using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddViewers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Viewers",
                table: "ForvaltningsObjektPropertiesMetadata",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Viewers",
                table: "ForvaltningsObjektMetadata",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Viewers",
                table: "ForvaltningsObjektPropertiesMetadata");

            migrationBuilder.DropColumn(
                name: "Viewers",
                table: "ForvaltningsObjektMetadata");
        }
    }
}
