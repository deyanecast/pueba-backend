namespace MiBackend.DTOs.Responses;

public class VentaResponse
{
    public int VentaId { get; set; }
    public string Cliente { get; set; }
    public string Observaciones { get; set; }
    public DateTime FechaVenta { get; set; }
    public decimal MontoTotal { get; set; }
    public string TipoVenta { get; set; }
    public List<VentaDetalleResponse> Detalles { get; set; }
}

public class VentaDetalleResponse
{
    public int DetalleVentaId { get; set; }
    public string TipoItem { get; set; }
    public ProductoResponse Producto { get; set; }
    public ComboResponse Combo { get; set; }
    public decimal CantidadLibras { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
} 