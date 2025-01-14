using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services
{
    public interface IComboService
    {
        Task<ComboResponse> CreateComboAsync(CreateComboRequest request);
        Task<List<ComboResponse>> GetCombosAsync();
        Task<ComboResponse> GetComboByIdAsync(int id);
        Task<List<ComboResponse>> GetActiveCombosAsync();
        Task<bool> UpdateComboStatusAsync(int id, bool isActive);
        Task<bool> ValidateComboStockAsync(int comboId, decimal cantidad);
        Task<decimal> CalculateComboTotalAsync(int comboId, decimal cantidad);
    }
} 