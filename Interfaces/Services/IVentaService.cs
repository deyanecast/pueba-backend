using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services
{
    public interface IVentaService
    {
        Task<VentaResponse?> GetVentaByIdAsync(int id);
        Task<List<VentaResponse>> GetVentasByDateAsync(DateTime date);
        Task<List<VentaResponse>> GetVentasByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalVentasByDateAsync(DateTime date);
        Task<VentaResponse> CreateVentaAsync(CreateVentaRequest request);
        Task<DashboardResponse> GetDashboardDataAsync();
    }
} 