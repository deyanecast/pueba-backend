using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class CreateProductoRequest
{
    [Required]
    public required string Nombre { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal CantidadLibras { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio por libra debe ser mayor a 0")]
    public decimal PrecioPorLibra { get; set; }

    [Required]
    public required string TipoEmpaque { get; set; }
} 