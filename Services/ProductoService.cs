using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Repositories;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Services;

public class ProductoService : IProductoService
{
    private readonly IGenericRepository<Producto> _productoRepository;
    private readonly ILogger<ProductoService> _logger;

    public ProductoService(IGenericRepository<Producto> productoRepository, ILogger<ProductoService> logger)
    {
        _productoRepository = productoRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductoResponse>> GetAllAsync()
    {
        var productos = await _productoRepository.GetAllAsync();
        return productos.Select(MapToResponse);
    }

    public async Task<ProductoResponse?> GetByIdAsync(int id)
    {
        var producto = await _productoRepository.GetByIdAsync(id);
        return producto != null ? MapToResponse(producto) : null;
    }

    public async Task<ProductoResponse> CreateAsync(CreateProductoRequest request)
    {
        // Verificar si ya existe un producto con el mismo nombre y tipo de empaque
        var productos = await _productoRepository.GetAllAsync();
        var existingProduct = productos.FirstOrDefault(p => 
            p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) &&
            p.TipoEmpaque == request.TipoEmpaque &&
            p.CantidadLibras == request.CantidadLibras);

        if (existingProduct != null)
        {
            _logger.LogWarning("Intento de crear producto duplicado: {Nombre}, {TipoEmpaque}, {CantidadLibras}", 
                request.Nombre, request.TipoEmpaque, request.CantidadLibras);
            throw new InvalidOperationException("Ya existe un producto con las mismas características");
        }

        var producto = new Producto
        {
            Nombre = request.Nombre,
            CantidadLibras = request.CantidadLibras,
            PrecioPorLibra = request.PrecioPorLibra,
            TipoEmpaque = request.TipoEmpaque,
            EstaActivo = true
        };

        var createdProducto = await _productoRepository.CreateAsync(producto);
        _logger.LogInformation("Producto creado exitosamente: {ProductoId}", createdProducto.ProductoId);
        return MapToResponse(createdProducto);
    }

    public async Task<ProductoResponse> UpdateAsync(int id, UpdateProductoRequest request)
    {
        var producto = await _productoRepository.GetByIdAsync(id);
        if (producto == null)
        {
            throw new KeyNotFoundException($"Producto con ID {id} no encontrado");
        }

        producto.Nombre = request.Nombre;
        producto.CantidadLibras = request.CantidadLibras;
        producto.PrecioPorLibra = request.PrecioPorLibra;
        producto.TipoEmpaque = request.TipoEmpaque;

        var updatedProducto = await _productoRepository.UpdateAsync(producto);
        return MapToResponse(updatedProducto);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _productoRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new KeyNotFoundException($"Producto con ID {id} no encontrado");
        }

        await _productoRepository.DeleteAsync(id);
    }

    public async Task<ProductoResponse> ToggleEstadoAsync(int id)
    {
        var producto = await _productoRepository.GetByIdAsync(id);
        if (producto == null)
        {
            throw new KeyNotFoundException($"Producto con ID {id} no encontrado");
        }

        producto.EstaActivo = !producto.EstaActivo;
        var updatedProducto = await _productoRepository.UpdateAsync(producto);
        return MapToResponse(updatedProducto);
    }

    public async Task<IEnumerable<ProductoResponse>> BuscarAsync(string? nombre, decimal? precioMin, decimal? precioMax, bool? activo)
    {
        var productos = await _productoRepository.GetAllAsync();
        
        var query = productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            query = query.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
        }

        if (precioMin.HasValue)
        {
            query = query.Where(p => p.PrecioPorLibra >= precioMin.Value);
        }

        if (precioMax.HasValue)
        {
            query = query.Where(p => p.PrecioPorLibra <= precioMax.Value);
        }

        if (activo.HasValue)
        {
            query = query.Where(p => p.EstaActivo == activo.Value);
        }

        return query.Select(MapToResponse);
    }

    private static ProductoResponse MapToResponse(Producto producto)
    {
        return new ProductoResponse
        {
            ProductoId = producto.ProductoId,
            Nombre = producto.Nombre,
            CantidadLibras = producto.CantidadLibras,
            PrecioPorLibra = producto.PrecioPorLibra,
            TipoEmpaque = producto.TipoEmpaque,
            EstaActivo = producto.EstaActivo,
            UltimaActualizacion = DateTime.UtcNow
        };
    }
} 