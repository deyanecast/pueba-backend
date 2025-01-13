using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services;

public interface IVentaService
{
    Task<IEnumerable<VentaResponse>> GetAllAsync();
    Task<VentaResponse?> GetByIdAsync(int id);
    Task<VentaResponse> CreateAsync(CreateVentaRequest request);
    Task<object> GenerarReporteAsync(DateTime? fechaInicio, DateTime? fechaFin, string? cliente);
} 