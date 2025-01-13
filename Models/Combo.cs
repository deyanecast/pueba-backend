using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiBackend.Models
{
    [Table("combos")]
    public class Combo
    {
        [Key]
        [Column("combo_id")]
        public int ComboId { get; set; }

        [Required]
        [Column("nombre")]
        public required string Nombre { get; set; }

        [Required]
        [Column("descripcion")]
        public required string Descripcion { get; set; }

        [Required]
        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("esta_activo")]
        public bool EstaActivo { get; set; } = true;

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ComboDetalle> Productos { get; set; } = new List<ComboDetalle>();
    }
} 