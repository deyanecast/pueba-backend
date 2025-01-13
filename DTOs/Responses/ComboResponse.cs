namespace MiBackend.DTOs.Responses;

public class ComboResponse
{
    public int ComboId { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool EstaActivo { get; set; }
    public required List<ComboProductoResponse> Productos { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class ComboProductoResponse
{
    public int ProductoId { get; set; }
    public required string NombreProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
} 