namespace MiBackend.DTOs.Responses;

public class ComboResponse
{
    public int ComboId { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime UltimaActualizacion { get; set; }
    public List<ComboDetalleResponse> Productos { get; set; }
}

public class ComboDetalleResponse
{
    public int ComboDetalleId { get; set; }
    public ProductoResponse Producto { get; set; }
    public decimal CantidadLibras { get; set; }
} 