using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class CreateComboRequest
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public required List<CreateComboDetalleRequest> Productos { get; set; }
}

public class CreateComboDetalleRequest
{
    public int ProductoId { get; set; }
    public decimal CantidadLibras { get; set; }
} 