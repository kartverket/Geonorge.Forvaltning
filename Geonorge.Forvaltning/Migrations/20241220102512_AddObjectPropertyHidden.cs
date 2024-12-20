﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    /// <inheritdoc />
    public partial class AddObjectPropertyHidden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "ForvaltningsObjektPropertiesMetadata",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "ForvaltningsObjektPropertiesMetadata");
        }
    }
}
