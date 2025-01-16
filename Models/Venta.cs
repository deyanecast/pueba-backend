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
        [StringLength(100)]
        public required string Cliente { get; set; }

        [Column("observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }

        [Required]
        [Column("tipo_venta")]
        [StringLength(20)]
        public string TipoVenta { get; set; } = "INDIVIDUAL";

        [Required]
        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; }

        [Required]
        [Column("monto_total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
    }
} 