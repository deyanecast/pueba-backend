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

        [Column("cliente")]
        public required string Cliente { get; set; }

        [Column("observaciones")]
        public required string Observaciones { get; set; }

        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; }

        [Column("monto_total")]
        public decimal MontoTotal { get; set; }

        [Column("tipo_venta")]
        public required string TipoVenta { get; set; }

        // Navigation property
        public List<VentaDetalle> VentaDetalles { get; set; } = new();
    }
} 