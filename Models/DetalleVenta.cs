using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiBackend.Models
{
    [Table("detalle_ventas")]
    public class DetalleVenta
    {
        [Key]
        [Column("detalle_venta_id")]
        public int DetalleVentaId { get; set; }

        [Column("venta_id")]
        public int VentaId { get; set; }

        [Column("producto_id")]
        public int? ProductoId { get; set; }

        [Column("combo_id")]
        public int? ComboId { get; set; }

        [Column("cantidad_libras")]
        public decimal? CantidadLibras { get; set; }

        [Required]
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        [ForeignKey("VentaId")]
        public virtual Venta Venta { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }
    }
} 