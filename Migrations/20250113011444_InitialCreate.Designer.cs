﻿// <auto-generated />
using System;
using MiBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250113011444_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MiBackend.Models.Producto", b =>
                {
                    b.Property<int>("ProductoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ProductoId"));

                    b.Property<decimal>("CantidadLibras")
                        .HasColumnType("numeric");

                    b.Property<bool>("EstaActivo")
                        .HasColumnType("boolean");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("PrecioPorLibra")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("TipoEmpaque")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ProductoId");

                    b.ToTable("Productos");
                });

            modelBuilder.Entity("MiBackend.Models.Venta", b =>
                {
                    b.Property<int>("VentaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("VentaId"));

                    b.Property<DateTime>("FechaVenta")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("MontoTotal")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("VentaId");

                    b.ToTable("Ventas");
                });
#pragma warning restore 612, 618
        }
    }
}
