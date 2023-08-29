using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddMatadataProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForvaltningsObjektPropertiesMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DataType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ForvaltningsObjektMetadataId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForvaltningsObjektPropertiesMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForvaltningsObjektPropertiesMetadata_ForvaltningsObjektMeta~",
                        column: x => x.ForvaltningsObjektMetadataId,
                        principalTable: "ForvaltningsObjektMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForvaltningsObjektPropertiesMetadata_ForvaltningsObjektMeta~",
                table: "ForvaltningsObjektPropertiesMetadata",
                column: "ForvaltningsObjektMetadataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForvaltningsObjektPropertiesMetadata");
        }
    }
}
