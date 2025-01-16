using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiBackend.Models
{
    [Table("venta_detalles")]
    public class VentaDetalle
    {
        [Key]
        [Column("detalle_venta_id")]
        public int VentaDetalleId { get; set; }

        [Column("venta_id")]
        public int VentaId { get; set; }

        [Column("tipo_item")]
        [StringLength(20)]
        public required string TipoItem { get; set; } = "PRODUCTO";

        [Column("cantidad_libras")]
        public decimal CantidadLibras { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        [Column("producto_id")]
        public int? ProductoId { get; set; }

        [Column("combo_id")]
        public int? ComboId { get; set; }

        public virtual Venta? Venta { get; set; }
        public virtual Producto? Producto { get; set; }
        public virtual Combo? Combo { get; set; }
    }
} 