using System;
using System.Collections.Generic;

namespace MiBackend.DTOs.Responses
{
    public class VentaResponse
    {
        public int VentaId { get; set; }
        public required string Cliente { get; set; }
        public string? Observaciones { get; set; }
        public required string TipoVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public List<VentaDetalleResponse> Detalles { get; set; } = new();
    }
} 