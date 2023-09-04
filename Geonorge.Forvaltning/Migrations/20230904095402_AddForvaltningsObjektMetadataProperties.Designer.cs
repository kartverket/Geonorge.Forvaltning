﻿// <auto-generated />
using Geonorge.Forvaltning.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Geonorge.Forvaltning.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20230904095402_AddForvaltningsObjektMetadataProperties")]
    partial class AddForvaltningsObjektMetadataProperties
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Organization")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasMaxLength(31)
                        .HasColumnType("character varying(31)");

                    b.HasKey("Id");

                    b.ToTable("ForvaltningsObjektMetadata");
                });

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektPropertiesMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ColumnName")
                        .IsRequired()
                        .HasMaxLength(31)
                        .HasColumnType("character varying(31)");

                    b.Property<string>("DataType")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("ForvaltningsObjektMetadataId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.HasIndex("ForvaltningsObjektMetadataId");

                    b.ToTable("ForvaltningsObjektPropertiesMetadata");
                });

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektPropertiesMetadata", b =>
                {
                    b.HasOne("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektMetadata", "ForvaltningsObjektMetadata")
                        .WithMany("ForvaltningsObjektPropertiesMetadata")
                        .HasForeignKey("ForvaltningsObjektMetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ForvaltningsObjektMetadata");
                });

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektMetadata", b =>
                {
                    b.Navigation("ForvaltningsObjektPropertiesMetadata");
                });
#pragma warning restore 612, 618
        }
    }
}
