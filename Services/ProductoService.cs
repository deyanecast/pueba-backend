using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces;
using MiBackend.Interfaces.Services;
using MiBackend.Models;
using Microsoft.Extensions.Logging;

namespace MiBackend.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductoService> _logger;
        private const string ALL_PRODUCTS_CACHE_KEY = "ALL_PRODUCTS";
        private const string ACTIVE_PRODUCTS_CACHE_KEY = "ACTIVE_PRODUCTS";

        public ProductoService(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<ProductoService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<ProductoResponse>> GetProductosAsync()
        {
            if (_cache.TryGetValue(ALL_PRODUCTS_CACHE_KEY, out List<ProductoResponse> cachedProducts))
            {
                return cachedProducts;
            }

            var productos = await _unitOfWork.Repository<Producto>()
                .Query()
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoResponse
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    CantidadLibras = p.CantidadLibras,
                    PrecioPorLibra = p.PrecioPorLibra,
                    TipoEmpaque = p.TipoEmpaque,
                    EstaActivo = p.EstaActivo,
                    UltimaActualizacion = p.UltimaActualizacion
                })
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(1);

            _cache.Set(ALL_PRODUCTS_CACHE_KEY, productos, cacheEntryOptions);

            return productos;
        }

        public async Task<List<ProductoResponse>> GetActiveProductosAsync()
        {
            if (_cache.TryGetValue(ACTIVE_PRODUCTS_CACHE_KEY, out List<ProductoResponse> cachedProducts))
            {
                return cachedProducts;
            }

            var productos = await _unitOfWork.Repository<Producto>()
                .Query()
                .Where(p => p.EstaActivo)
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoResponse
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    CantidadLibras = p.CantidadLibras,
                    PrecioPorLibra = p.PrecioPorLibra,
                    TipoEmpaque = p.TipoEmpaque,
                    EstaActivo = p.EstaActivo,
                    UltimaActualizacion = p.UltimaActualizacion
                })
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(1);

            _cache.Set(ACTIVE_PRODUCTS_CACHE_KEY, productos, cacheEntryOptions);

            return productos;
        }

        public async Task<ProductoResponse> GetProductoByIdAsync(int id)
        {
            var cacheKey = $"PRODUCT_{id}";
            if (_cache.TryGetValue(cacheKey, out ProductoResponse cachedProduct))
            {
                return cachedProduct;
            }

            var producto = await _unitOfWork.Repository<Producto>()
                .Query()
                .Where(p => p.ProductoId == id)
                .Select(p => new ProductoResponse
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    CantidadLibras = p.CantidadLibras,
                    PrecioPorLibra = p.PrecioPorLibra,
                    TipoEmpaque = p.TipoEmpaque,
                    EstaActivo = p.EstaActivo,
                    UltimaActualizacion = p.UltimaActualizacion
                })
                .FirstOrDefaultAsync();

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(1);

            _cache.Set(cacheKey, producto, cacheEntryOptions);

            return producto;
        }

        public async Task<ProductoResponse> CreateProductoAsync(CreateProductoRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var producto = new Producto
                {
                    Nombre = request.Nombre,
                    CantidadLibras = request.CantidadLibras,
                    PrecioPorLibra = request.PrecioPorLibra,
                    TipoEmpaque = request.TipoEmpaque,
                    EstaActivo = true,
                    UltimaActualizacion = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Producto>().AddAsync(producto);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                InvalidateCache();

                return new ProductoResponse
                {
                    ProductoId = producto.ProductoId,
                    Nombre = producto.Nombre,
                    CantidadLibras = producto.CantidadLibras,
                    PrecioPorLibra = producto.PrecioPorLibra,
                    TipoEmpaque = producto.TipoEmpaque,
                    EstaActivo = producto.EstaActivo,
                    UltimaActualizacion = producto.UltimaActualizacion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ProductoResponse> UpdateProductoAsync(int id, UpdateProductoRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var producto = await _unitOfWork.Repository<Producto>().GetByIdAsync(id);
                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

                producto.Nombre = request.Nombre;
                producto.TipoEmpaque = request.TipoEmpaque;
                producto.EstaActivo = request.EstaActivo;
                producto.UltimaActualizacion = DateTime.UtcNow;

                _unitOfWork.Repository<Producto>().Update(producto);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                InvalidateCache();

                return new ProductoResponse
                {
                    ProductoId = producto.ProductoId,
                    Nombre = producto.Nombre,
                    CantidadLibras = producto.CantidadLibras,
                    PrecioPorLibra = producto.PrecioPorLibra,
                    TipoEmpaque = producto.TipoEmpaque,
                    EstaActivo = producto.EstaActivo,
                    UltimaActualizacion = producto.UltimaActualizacion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateProductoStatusAsync(int id, bool isActive)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var producto = await _unitOfWork.Repository<Producto>().GetByIdAsync(id);
                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

                producto.EstaActivo = isActive;
                producto.UltimaActualizacion = DateTime.UtcNow;

                _unitOfWork.Repository<Producto>().Update(producto);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                InvalidateCache();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del producto");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ValidateProductoStockAsync(int productoId, decimal cantidadRequerida)
        {
            var producto = await _unitOfWork.Repository<Producto>()
                .Query()
                .Where(p => p.ProductoId == productoId)
                .FirstOrDefaultAsync();

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            if (!producto.EstaActivo)
                throw new InvalidOperationException($"El producto {producto.Nombre} no está activo");

            return producto.CantidadLibras >= cantidadRequerida;
        }

        public async Task<decimal> CalculateProductoTotalAsync(int productoId, decimal cantidad)
        {
            var producto = await _unitOfWork.Repository<Producto>()
                .Query()
                .FirstOrDefaultAsync(p => p.ProductoId == productoId);

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            if (!producto.EstaActivo)
                throw new InvalidOperationException($"El producto no está activo");

            return producto.PrecioPorLibra * cantidad;
        }

        public async Task<decimal> GetPrecioProductoAsync(int productoId)
        {
            var producto = await _unitOfWork.Repository<Producto>()
                .Query()
                .Where(p => p.ProductoId == productoId)
                .Select(p => new { p.PrecioPorLibra })
                .FirstOrDefaultAsync();

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            return producto.PrecioPorLibra;
        }

        public async Task<bool> UpdateProductoStockAsync(int productoId, decimal cantidadAjuste)
        {
            try
            {
                var producto = await _unitOfWork.Repository<Producto>().GetByIdAsync(productoId);
                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

                if (!producto.EstaActivo)
                    throw new InvalidOperationException($"El producto con ID {productoId} no está activo");

                var nuevoStock = producto.CantidadLibras + cantidadAjuste;
                if (nuevoStock < 0)
                    throw new InvalidOperationException($"Stock insuficiente para el producto con ID {productoId}");

                producto.CantidadLibras = nuevoStock;
                producto.UltimaActualizacion = DateTime.UtcNow;

                _unitOfWork.Repository<Producto>().Update(producto);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {ProductoId}", productoId);
                throw;
            }
        }

        private void InvalidateCache()
        {
            _cache.Remove(ALL_PRODUCTS_CACHE_KEY);
            _cache.Remove(ACTIVE_PRODUCTS_CACHE_KEY);
        }
    }
} 