using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class CreateComboRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
    public required string Nombre { get; set; }

    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
    public required string Descripcion { get; set; }

    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, 10000, ErrorMessage = "El precio debe estar entre 0.01 y 10000")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "Se requiere al menos un producto en el combo")]
    [MinLength(1, ErrorMessage = "El combo debe tener al menos un producto")]
    public required List<ComboProductoRequest> Productos { get; set; }
}

public class ComboProductoRequest
{
    [Required(ErrorMessage = "El ID del producto es requerido")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
    public int Cantidad { get; set; }
} 