using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddForvaltningsObjektMetadataProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColumnName",
                table: "ForvaltningsObjektPropertiesMetadata",
                type: "character varying(31)",
                maxLength: 31,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "ForvaltningsObjektMetadata",
                type: "character varying(31)",
                maxLength: 31,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnName",
                table: "ForvaltningsObjektPropertiesMetadata");

            migrationBuilder.DropColumn(
                name: "TableName",
                table: "ForvaltningsObjektMetadata");
        }
    }
}
