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
            try
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
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(ALL_PRODUCTS_CACHE_KEY, productos, cacheEntryOptions);

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los productos");
                throw new InvalidOperationException("Error al obtener productos. Por favor, inténtelo de nuevo.", ex);
            }
        }

        public async Task<List<ProductoResponse>> GetActiveProductosAsync()
        {
            try
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
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(ACTIVE_PRODUCTS_CACHE_KEY, productos, cacheEntryOptions);

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos activos");
                throw new InvalidOperationException("Error al obtener productos activos. Por favor, inténtelo de nuevo.", ex);
            }
        }

        public async Task<ProductoResponse> GetProductoByIdAsync(int id)
        {
            try
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
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, producto, cacheEntryOptions);

                return producto;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error al obtener producto por ID {ProductoId}", id);
                throw new InvalidOperationException($"Error al obtener producto: {ex.Message}", ex);
            }
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
            try
            {
                var producto = await _unitOfWork.Repository<Producto>()
                    .Query()
                    .Where(p => p.ProductoId == productoId)
                    .Select(p => new { p.CantidadLibras, p.EstaActivo })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

                if (!producto.EstaActivo)
                    throw new InvalidOperationException($"El producto con ID {productoId} no está activo");

                return producto.CantidadLibras >= cantidadRequerida;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Error al validar stock del producto {ProductoId}", productoId);
                throw new InvalidOperationException($"Error al validar stock: {ex.Message}", ex);
            }
        }

        public async Task<decimal> CalculateProductoTotalAsync(int productoId, decimal cantidad)
        {
            try
            {
                var producto = await _unitOfWork.Repository<Producto>()
                    .Query()
                    .Where(p => p.ProductoId == productoId)
                    .Select(p => new { p.PrecioPorLibra, p.EstaActivo })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

                if (!producto.EstaActivo)
                    throw new InvalidOperationException($"El producto no está activo");

                return producto.PrecioPorLibra * cantidad;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Error al calcular total del producto {ProductoId}", productoId);
                throw new InvalidOperationException($"Error al calcular total: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetPrecioProductoAsync(int productoId)
        {
            try
            {
                var cacheKey = $"PRODUCT_PRICE_{productoId}";
                if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
                {
                    return cachedPrice;
                }

                var producto = await _unitOfWork.Repository<Producto>()
                    .Query()
                    .Where(p => p.ProductoId == productoId)
                    .Select(p => new { p.PrecioPorLibra })
                    .FirstOrDefaultAsync();

                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, producto.PrecioPorLibra, cacheEntryOptions);

                return producto.PrecioPorLibra;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error al obtener precio del producto {ProductoId}", productoId);
                throw new InvalidOperationException($"Error al obtener precio: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateProductoStockAsync(int productoId, decimal cantidadAjuste)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var producto = await _unitOfWork.Repository<Producto>()
                    .Query()
                    .Where(p => p.ProductoId == productoId)
                    .FirstOrDefaultAsync();

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
                await _unitOfWork.CommitTransactionAsync();

                // Invalidar caché relacionado
                InvalidateCache();
                _cache.Remove($"PRODUCT_{productoId}");
                _cache.Remove($"PRODUCT_PRICE_{productoId}");

                return true;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error al actualizar stock del producto {ProductoId}", productoId);
                throw new InvalidOperationException($"Error al actualizar stock: {ex.Message}", ex);
            }
        }

        public async Task<List<ProductoStockBajoResponse>> GetProductosStockBajoAsync()
        {
            try
            {
                const string cacheKey = "PRODUCTOS_STOCK_BAJO";
                if (_cache.TryGetValue(cacheKey, out List<ProductoStockBajoResponse> cachedProducts))
                {
                    return cachedProducts;
                }

                var productos = await _unitOfWork.Repository<Producto>()
                    .Query()
                    .Where(p => p.CantidadLibras < 10 && p.EstaActivo)
                    .Select(p => new ProductoStockBajoResponse
                    {
                        ProductoId = p.ProductoId,
                        Nombre = p.Nombre,
                        CantidadLibras = p.CantidadLibras
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, productos, cacheEntryOptions);

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con stock bajo");
                throw new InvalidOperationException("Error al obtener productos con stock bajo. Por favor, inténtelo de nuevo.", ex);
            }
        }

        private void InvalidateCache()
        {
            _cache.Remove(ALL_PRODUCTS_CACHE_KEY);
            _cache.Remove(ACTIVE_PRODUCTS_CACHE_KEY);
        }
    }
} 