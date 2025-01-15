using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiBackend.Models
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("nombre")]
        public required string Nombre { get; set; }

        [Column("cantidad_libras")]
        public decimal CantidadLibras { get; set; }

        [Column("precio_por_libra")]
        public decimal PrecioPorLibra { get; set; }

        [Column("tipo_empaque")]
        public required string TipoEmpaque { get; set; }

        [Column("esta_activo")]
        public bool EstaActivo { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }

        // Navigation properties
        public List<VentaDetalle> VentaDetalles { get; set; } = new();
        public List<ComboDetalle> ComboDetalles { get; set; } = new();
    }
} 