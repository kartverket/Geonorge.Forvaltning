﻿// <auto-generated />
using System;
using System.Collections.Generic;
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
    [Migration("20250416103931_AddPropertyOrder")]
    partial class AddPropertyOrder
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.AccessByProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<List<string>>("Contributors")
                        .HasColumnType("text[]");

                    b.Property<int?>("ForvaltningsObjektPropertiesMetadataId")
                        .HasColumnType("integer");

                    b.Property<string>("Organization")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ForvaltningsObjektPropertiesMetadataId");

                    b.ToTable("AccessByProperties");
                });

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<List<int>>("AttachedForvaltningObjektMetadataIds")
                        .HasColumnType("integer[]");

                    b.Property<List<string>>("Contributors")
                        .HasColumnType("text[]");

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<bool>("IsOpenData")
                        .HasColumnType("boolean");

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

                    b.Property<List<string>>("Viewers")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("ForvaltningsObjektMetadata");
                });

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektPropertiesMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<List<string>>("AllowedValues")
                        .HasColumnType("text[]");

                    b.Property<string>("ColumnName")
                        .IsRequired()
                        .HasMaxLength(31)
                        .HasColumnType("character varying(31)");

                    b.Property<List<string>>("Contributors")
                        .HasColumnType("text[]");

                    b.Property<string>("DataType")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("ForvaltningsObjektMetadataId")
                        .HasColumnType("integer");

                    b.Property<bool>("Hidden")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("OrganizationNumber")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("PropertyOrder")
                        .HasColumnType("integer");

                    b.Property<List<string>>("Viewers")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.HasIndex("ForvaltningsObjektMetadataId");

                    b.ToTable("ForvaltningsObjektPropertiesMetadata");
                });

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.AccessByProperties", b =>
                {
                    b.HasOne("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektPropertiesMetadata", "ForvaltningsObjektPropertiesMetadata")
                        .WithMany("AccessByProperties")
                        .HasForeignKey("ForvaltningsObjektPropertiesMetadataId");

                    b.Navigation("ForvaltningsObjektPropertiesMetadata");
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

            modelBuilder.Entity("Geonorge.Forvaltning.Models.Entity.ForvaltningsObjektPropertiesMetadata", b =>
                {
                    b.Navigation("AccessByProperties");
                });
#pragma warning restore 612, 618
        }
    }
}
