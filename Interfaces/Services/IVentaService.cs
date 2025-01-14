using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services
{
    public interface IVentaService
    {
        Task<VentaResponse> CreateVentaAsync(CreateVentaRequest request);
        Task<List<VentaResponse>> GetVentasAsync();
        Task<VentaResponse> GetVentaByIdAsync(int id);
        Task<List<VentaResponse>> GetVentasByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalVentasByDateAsync(DateTime date);
    }
} 