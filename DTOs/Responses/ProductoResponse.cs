namespace MiBackend.DTOs.Responses;

public class ProductoResponse
{
    public int ProductoId { get; set; }
    public required string Nombre { get; set; }
    public decimal CantidadLibras { get; set; }
    public decimal PrecioPorLibra { get; set; }
    public required string TipoEmpaque { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime UltimaActualizacion { get; set; }
} 