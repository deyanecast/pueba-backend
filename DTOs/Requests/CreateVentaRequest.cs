using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class CreateVentaRequest
{
    [Required(ErrorMessage = "El cliente es requerido")]
    [StringLength(100, ErrorMessage = "El nombre del cliente no puede tener más de 100 caracteres")]
    public required string Cliente { get; set; }

    [Required(ErrorMessage = "Se requiere al menos un producto o combo")]
    [MinLength(1, ErrorMessage = "La venta debe tener al menos un ítem")]
    public required List<VentaItemRequest> Items { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden tener más de 500 caracteres")]
    public string? Observaciones { get; set; }
}

public class VentaItemRequest
{
    [Required(ErrorMessage = "El tipo de ítem es requerido")]
    public required string TipoItem { get; set; } // "Producto" o "Combo"

    [Required(ErrorMessage = "El ID del ítem es requerido")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000")]
    public int Cantidad { get; set; }
} 