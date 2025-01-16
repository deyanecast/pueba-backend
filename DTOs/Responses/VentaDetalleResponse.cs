namespace MiBackend.DTOs.Responses
{
    public class VentaDetalleResponse
    {
        public int VentaDetalleId { get; set; }
        public string TipoItem { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal Total { get; set; }
    }
} 