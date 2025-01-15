namespace MiBackend.DTOs.Responses;

public class ComboResponse
{
    public int ComboId { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime UltimaActualizacion { get; set; }
    public required List<ComboDetalleResponse> Productos { get; set; }
}

public class ComboDetalleResponse
{
    public int ComboDetalleId { get; set; }
    public required ProductoResponse Producto { get; set; }
    public decimal CantidadLibras { get; set; }
} 