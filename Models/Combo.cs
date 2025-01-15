using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiBackend.Models
{
    [Table("combos")]
    public class Combo
    {
        public Combo()
        {
            ComboDetalles = new List<ComboDetalle>();
            VentaDetalles = new List<VentaDetalle>();
        }

        [Key]
        [Column("combo_id")]
        public int ComboId { get; set; }

        [Column("nombre")]
        public required string Nombre { get; set; }

        [Column("descripcion")]
        public required string Descripcion { get; set; }

        [Column("precio")]
        public required decimal Precio { get; set; }

        [Column("esta_activo")]
        public bool EstaActivo { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }

        // Navigation properties
        public virtual ICollection<ComboDetalle> ComboDetalles { get; set; }
        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; }
    }
} 