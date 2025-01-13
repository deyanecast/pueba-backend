namespace MiBackend.DTOs.Responses;

public class VentaResponse
{
    public int VentaId { get; set; }
    public required string Cliente { get; set; }
    public required List<VentaItemResponse> Items { get; set; }
    public string? Observaciones { get; set; }
    public decimal Total => Items.Sum(i => i.Subtotal);
    public DateTime FechaVenta { get; set; }
}

public class VentaItemResponse
{
    public required string TipoItem { get; set; }
    public int ItemId { get; set; }
    public required string Nombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
} 