using System.ComponentModel.DataAnnotations;

namespace MiBackend.Models
{
    public class ComboDetalle
    {
        public int ComboDetalleId { get; set; }
        public required int ComboId { get; set; }
        public required int ProductoId { get; set; }
        public required decimal CantidadLibras { get; set; }

        // Navegación
        public virtual Combo? Combo { get; set; }
        public virtual Producto? Producto { get; set; }
    }
} 