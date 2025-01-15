namespace MiBackend.DTOs.Responses;

public class VentaResponse
{
    public int VentaId { get; set; }
    public required string Cliente { get; set; }
    public required string Observaciones { get; set; }
    public DateTime FechaVenta { get; set; }
    public decimal MontoTotal { get; set; }
    public required string TipoVenta { get; set; }
    public required List<VentaDetalleResponse> Detalles { get; set; }
}

public class VentaDetalleResponse
{
    public int DetalleVentaId { get; set; }
    public required string TipoItem { get; set; }
    public ProductoResponse? Producto { get; set; }
    public ComboResponse? Combo { get; set; }
    public decimal CantidadLibras { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
} 