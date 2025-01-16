namespace MiBackend.DTOs.Requests
{
    public class CreateVentaDetalleRequest
    {
        public required string TipoItem { get; set; }
        public int ItemId { get; set; }
        public decimal Cantidad { get; set; }
    }
} 