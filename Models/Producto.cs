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
        public string Nombre { get; set; }

        [Column("cantidad_libras")]
        public decimal CantidadLibras { get; set; }

        [Column("precio_por_libra")]
        public decimal PrecioPorLibra { get; set; }

        [Column("tipo_empaque")]
        public string TipoEmpaque { get; set; }

        [Column("esta_activo")]
        public bool EstaActivo { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }

        // Navigation properties
        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; }
        public virtual ICollection<ComboDetalle> ComboDetalles { get; set; }
    }
} 