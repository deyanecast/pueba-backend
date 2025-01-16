using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;

namespace MiBackend.Interfaces.Services
{
    public interface IProductoService
    {
        Task<List<ProductoResponse>> GetProductosAsync();
        Task<List<ProductoResponse>> GetActiveProductosAsync();
        Task<ProductoResponse> GetProductoByIdAsync(int id);
        Task<ProductoResponse> CreateProductoAsync(CreateProductoRequest request);
        Task<ProductoResponse> UpdateProductoAsync(int id, UpdateProductoRequest request);
        Task<bool> UpdateProductoStatusAsync(int id, bool isActive);
        Task<bool> ValidateProductoStockAsync(int productoId, decimal cantidadRequerida);
        Task<decimal> CalculateProductoTotalAsync(int productoId, decimal cantidad);
        Task<decimal> GetPrecioProductoAsync(int productoId);
        Task<bool> UpdateProductoStockAsync(int productoId, decimal cantidadAjuste);
    }
} 