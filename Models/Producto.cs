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
        
        [Required]
        [Column("nombre")]
        public required string Nombre { get; set; }
        
        [Required]
        [Column("cantidad_libras")]
        public decimal CantidadLibras { get; set; }
        
        [Required]
        [Column("precio_por_libra")]
        public decimal PrecioPorLibra { get; set; }
        
        [Column("tipo_empaque")]
        public string? TipoEmpaque { get; set; }
        
        [Column("esta_activo")]
        public bool EstaActivo { get; set; } = true;
    }
} 