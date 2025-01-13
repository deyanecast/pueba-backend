using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiBackend.Models
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        [Column("venta_id")]
        public int VentaId { get; set; }

        [Required]
        [Column("cliente")]
        public required string Cliente { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("monto_total")]
        public decimal MontoTotal { get; set; }

        public virtual ICollection<VentaItem> Items { get; set; } = new List<VentaItem>();
    }

    [Table("venta_items")]
    public class VentaItem
    {
        [Key]
        [Column("venta_item_id")]
        public int VentaItemId { get; set; }

        [Column("venta_id")]
        public int VentaId { get; set; }

        [Required]
        [Column("tipo_item")]
        public required string TipoItem { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [ForeignKey("VentaId")]
        public virtual Venta Venta { get; set; } = null!;
    }
} 