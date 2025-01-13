using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services;

public interface IComboService
{
    Task<IEnumerable<ComboResponse>> GetAllAsync();
    Task<ComboResponse?> GetByIdAsync(int id);
    Task<ComboResponse> CreateAsync(CreateComboRequest request);
    Task<ComboResponse> UpdateAsync(int id, CreateComboRequest request);
    Task DeleteAsync(int id);
    Task<ComboResponse> ToggleEstadoAsync(int id);
} 