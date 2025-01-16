using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class CreateVentaRequest
{
    public required string Cliente { get; set; }
    public required string Observaciones { get; set; }
    public required string TipoVenta { get; set; }
    public required List<CreateVentaDetalleRequest> Detalles { get; set; }
} 