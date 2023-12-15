using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddDataContributors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Contributors",
                table: "ForvaltningsObjektPropertiesMetadata",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Contributors",
                table: "ForvaltningsObjektMetadata",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contributors",
                table: "ForvaltningsObjektPropertiesMetadata");

            migrationBuilder.DropColumn(
                name: "Contributors",
                table: "ForvaltningsObjektMetadata");
        }
    }
}
