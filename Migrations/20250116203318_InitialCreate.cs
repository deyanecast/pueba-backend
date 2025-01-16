using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "combos",
                columns: table => new
                {
                    combo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    precio = table.Column<decimal>(type: "numeric", nullable: false),
                    esta_activo = table.Column<bool>(type: "boolean", nullable: false),
                    ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_combos", x => x.combo_id);
                    table.CheckConstraint("CK_Combo_Precio", "\"Precio\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    producto_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    cantidad_libras = table.Column<decimal>(type: "numeric", nullable: false),
                    precio_por_libra = table.Column<decimal>(type: "numeric", nullable: false),
                    tipo_empaque = table.Column<string>(type: "text", nullable: false),
                    esta_activo = table.Column<bool>(type: "boolean", nullable: false),
                    ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.producto_id);
                    table.CheckConstraint("CK_Producto_CantidadLibras", "\"CantidadLibras\" >= 0");
                    table.CheckConstraint("CK_Producto_PrecioPorLibra", "\"PrecioPorLibra\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                columns: table => new
                {
                    venta_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tipo_venta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_venta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    monto_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.venta_id);
                });

            migrationBuilder.CreateTable(
                name: "combo_detalles",
                columns: table => new
                {
                    combo_detalle_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    combo_id = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad_libras = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("combo_detalles_pkey", x => x.combo_detalle_id);
                    table.ForeignKey(
                        name: "FK_combo_detalles_combos_combo_id",
                        column: x => x.combo_id,
                        principalTable: "combos",
                        principalColumn: "combo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_combo_detalles_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "producto_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "venta_detalles",
                columns: table => new
                {
                    detalle_venta_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    venta_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_item = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cantidad_libras = table.Column<decimal>(type: "numeric", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    producto_id = table.Column<int>(type: "integer", nullable: true),
                    combo_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta_detalles", x => x.detalle_venta_id);
                    table.CheckConstraint("CK_VentaDetalle_TipoItem", "(tipo_item = 'PRODUCTO' AND producto_id IS NOT NULL AND combo_id IS NULL) OR (tipo_item = 'COMBO' AND combo_id IS NOT NULL AND producto_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_venta_detalles_combos_combo_id",
                        column: x => x.combo_id,
                        principalTable: "combos",
                        principalColumn: "combo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venta_detalles_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "producto_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venta_detalles_ventas_venta_id",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "venta_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_combo_detalles_combo_id_producto_id",
                table: "combo_detalles",
                columns: new[] { "combo_id", "producto_id" });

            migrationBuilder.CreateIndex(
                name: "IX_combo_detalles_producto_id",
                table: "combo_detalles",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_combos_esta_activo",
                table: "combos",
                column: "esta_activo");

            migrationBuilder.CreateIndex(
                name: "IX_productos_esta_activo",
                table: "productos",
                column: "esta_activo");

            migrationBuilder.CreateIndex(
                name: "IX_venta_detalles_combo_id",
                table: "venta_detalles",
                column: "combo_id");

            migrationBuilder.CreateIndex(
                name: "IX_venta_detalles_producto_id",
                table: "venta_detalles",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_venta_detalles_venta_id_tipo_item",
                table: "venta_detalles",
                columns: new[] { "venta_id", "tipo_item" });

            migrationBuilder.CreateIndex(
                name: "IX_ventas_fecha_venta",
                table: "ventas",
                column: "fecha_venta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "combo_detalles");

            migrationBuilder.DropTable(
                name: "venta_detalles");

            migrationBuilder.DropTable(
                name: "combos");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "ventas");
        }
    }
}
