using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Models;

namespace MiBackend.Interfaces.Services;

public interface IProductoService
{
    Task<IEnumerable<ProductoResponse>> GetAllAsync();
    Task<ProductoResponse?> GetByIdAsync(int id);
    Task<ProductoResponse> CreateAsync(CreateProductoRequest request);
    Task<ProductoResponse> UpdateAsync(int id, UpdateProductoRequest request);
    Task DeleteAsync(int id);
    Task<ProductoResponse> ToggleEstadoAsync(int id);
    Task<IEnumerable<ProductoResponse>> BuscarAsync(string? nombre, decimal? precioMin, decimal? precioMax, bool? activo);
} 