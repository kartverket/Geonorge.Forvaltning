using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessByProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessByProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Contributors = table.Column<List<string>>(type: "text[]", nullable: true),
                    ForvaltningsObjektPropertiesMetadataId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessByProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessByProperties_ForvaltningsObjektPropertiesMetadata_For~",
                        column: x => x.ForvaltningsObjektPropertiesMetadataId,
                        principalTable: "ForvaltningsObjektPropertiesMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessByProperties_ForvaltningsObjektPropertiesMetadataId",
                table: "AccessByProperties",
                column: "ForvaltningsObjektPropertiesMetadataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessByProperties");
        }
    }
}
