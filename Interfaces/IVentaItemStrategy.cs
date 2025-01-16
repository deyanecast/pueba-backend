using MiBackend.DTOs.Requests;
using MiBackend.Models;

namespace MiBackend.Interfaces
{
    public interface IVentaItemStrategy
    {
        Task<VentaDetalle> ProcessVentaDetalleAsync(Venta venta, CreateVentaDetalleRequest detalle);
    }
} 