namespace MiBackend.DTOs.Responses;

public class DashboardResponse
{
    public decimal VentasDelDia { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public List<ProductoStockBajoResponse> ProductosStockBajo { get; set; } = new();
    public int TotalProductosActivos { get; set; }
}

public class ProductoStockBajoResponse
{
    public int ProductoId { get; set; }
    public required string Nombre { get; set; }
    public decimal CantidadLibras { get; set; }
} 