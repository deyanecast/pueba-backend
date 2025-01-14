using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class CreateComboRequest
{
    [Required]
    public string Nombre { get; set; }

    [Required]
    public string Descripcion { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [Required]
    public List<ComboDetalleRequest> Productos { get; set; }
}

public class ComboDetalleRequest
{
    [Required]
    public int ProductoId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal CantidadLibras { get; set; }
} 