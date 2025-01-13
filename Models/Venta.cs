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

        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("monto_total")]
        public decimal MontoTotal { get; set; }

        public virtual ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
    }
} 