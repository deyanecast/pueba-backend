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

        [Required]
        [Column("cantidad_libras")]
        public decimal CantidadLibras { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo Combo { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; } = null!;
    }
} 