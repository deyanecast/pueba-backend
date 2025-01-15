using MiBackend.DTOs.Requests;
using MiBackend.Models;

namespace MiBackend.Strategies
{
    public interface IVentaItemStrategy
    {
        Task<VentaDetalle> ProcessVentaDetalleAsync(Venta venta, CreateVentaDetalleRequest detalle);
    }
} 