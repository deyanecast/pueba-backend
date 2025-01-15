using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiBackend.Models
{
    [Table("combo_detalles")]
    public class ComboDetalle
    {
        [Key]
        [Column("combo_detalle_id")]
        public int ComboDetalleId { get; set; }

        [Column("combo_id")]
        public int ComboId { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("cantidad_libras")]
        public decimal CantidadLibras { get; set; }

        // Navigation properties
        public required Combo Combo { get; set; }
        public required Producto Producto { get; set; }
    }
} 