using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationToProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationNumber",
                table: "ForvaltningsObjektPropertiesMetadata",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationNumber",
                table: "ForvaltningsObjektPropertiesMetadata");
        }
    }
}
