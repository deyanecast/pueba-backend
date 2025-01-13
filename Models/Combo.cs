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
        [Column("nombre_combo")]
        public required string NombreCombo { get; set; }

        [Required]
        [Column("precio_combo")]
        public decimal PrecioCombo { get; set; }

        public virtual ICollection<ComboDetalle> ComboDetalles { get; set; } = new List<ComboDetalle>();
    }
} 