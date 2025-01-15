using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class UpdateProductoRequest
{
    public required string Nombre { get; set; }
    public decimal CantidadLibras { get; set; }
    public decimal PrecioPorLibra { get; set; }
    public required string TipoEmpaque { get; set; }
    public bool EstaActivo { get; set; }
} 