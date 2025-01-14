using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class CreateVentaRequest
{
    [Required]
    public string Cliente { get; set; }

    public string Observaciones { get; set; }

    [Required]
    public string TipoVenta { get; set; }

    [Required]
    public List<VentaDetalleRequest> Detalles { get; set; }
}

public class VentaDetalleRequest
{
    [Required]
    public string TipoItem { get; set; }  // "PRODUCTO" o "COMBO"

    public int? ProductoId { get; set; }
    
    public int? ComboId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal CantidadLibras { get; set; }
} 