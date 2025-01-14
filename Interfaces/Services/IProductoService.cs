using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services
{
    public interface IProductoService
    {
        Task<ProductoResponse> CreateProductoAsync(CreateProductoRequest request);
        Task<List<ProductoResponse>> GetProductosAsync();
        Task<ProductoResponse> GetProductoByIdAsync(int id);
        Task<List<ProductoResponse>> GetActiveProductosAsync();
        Task<ProductoResponse> UpdateProductoAsync(int id, UpdateProductoRequest request);
        Task<bool> UpdateProductoStatusAsync(int id, bool isActive);
        Task<bool> ValidateProductoStockAsync(int productoId, decimal cantidadLibras);
        Task<bool> UpdateProductoStockAsync(int productoId, decimal cantidadLibras, bool isAddition);
    }
} 