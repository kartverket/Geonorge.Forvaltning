using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachedForvaltningsObjektMetadataIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessByProperties_ForvaltningsObjektPropertiesMetadata_For~",
                table: "AccessByProperties");

            migrationBuilder.DropColumn(
                name: "srid",
                table: "ForvaltningsObjektMetadata");

            migrationBuilder.AddColumn<List<int>>(
                name: "AttachedForvaltningObjektMetadataIds",
                table: "ForvaltningsObjektMetadata",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AccessByProperties",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Organization",
                table: "AccessByProperties",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "ForvaltningsObjektPropertiesMetadataId",
                table: "AccessByProperties",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessByProperties_ForvaltningsObjektPropertiesMetadata_For~",
                table: "AccessByProperties",
                column: "ForvaltningsObjektPropertiesMetadataId",
                principalTable: "ForvaltningsObjektPropertiesMetadata",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessByProperties_ForvaltningsObjektPropertiesMetadata_For~",
                table: "AccessByProperties");

            migrationBuilder.DropColumn(
                name: "AttachedForvaltningObjektMetadataIds",
                table: "ForvaltningsObjektMetadata");

            migrationBuilder.AddColumn<int>(
                name: "srid",
                table: "ForvaltningsObjektMetadata",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AccessByProperties",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Organization",
                table: "AccessByProperties",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ForvaltningsObjektPropertiesMetadataId",
                table: "AccessByProperties",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessByProperties_ForvaltningsObjektPropertiesMetadata_For~",
                table: "AccessByProperties",
                column: "ForvaltningsObjektPropertiesMetadataId",
                principalTable: "ForvaltningsObjektPropertiesMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
