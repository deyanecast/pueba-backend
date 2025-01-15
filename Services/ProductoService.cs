using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string ALL_PRODUCTS_CACHE_KEY = "AllProducts";
        private const string ACTIVE_PRODUCTS_CACHE_KEY = "ActiveProducts";

        public ProductoService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<ProductoResponse>> GetProductosAsync()
        {
            if (_cache.TryGetValue(ALL_PRODUCTS_CACHE_KEY, out List<ProductoResponse>? cachedProducts) && cachedProducts != null)
            {
                return cachedProducts;
            }

            var productos = await _context.Productos
                .AsNoTracking()
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
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(100);

            _cache.Set(ALL_PRODUCTS_CACHE_KEY, productos, cacheOptions);

            return productos;
        }

        public async Task<List<ProductoResponse>> GetActiveProductosAsync()
        {
            if (_cache.TryGetValue(ACTIVE_PRODUCTS_CACHE_KEY, out List<ProductoResponse>? cachedProducts) && cachedProducts != null)
            {
                return cachedProducts;
            }

            var productos = await _context.Productos
                .Where(p => p.EstaActivo)
                .AsNoTracking()
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
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(50);

            _cache.Set(ACTIVE_PRODUCTS_CACHE_KEY, productos, cacheOptions);

            return productos;
        }

        public async Task<ProductoResponse> GetProductoByIdAsync(int id)
        {
            string cacheKey = $"Producto_{id}";
            
            if (_cache.TryGetValue(cacheKey, out ProductoResponse? cachedProduct) && cachedProduct != null)
            {
                return cachedProduct;
            }

            var producto = await _context.Productos
                .AsNoTracking()
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
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(1);

            _cache.Set(cacheKey, producto, cacheOptions);

            return producto;
        }

        public async Task<bool> ValidateProductoStockAsync(int productoId, decimal cantidadLibras)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .Where(p => p.ProductoId == productoId)
                .Select(p => new { p.EstaActivo, p.CantidadLibras })
                .FirstOrDefaultAsync();

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            return producto.EstaActivo && producto.CantidadLibras >= cantidadLibras;
        }

        public async Task<ProductoResponse> CreateProductoAsync(CreateProductoRequest request)
        {
            var producto = new Producto
            {
                Nombre = request.Nombre,
                CantidadLibras = request.CantidadLibras,
                PrecioPorLibra = request.PrecioPorLibra,
                TipoEmpaque = request.TipoEmpaque,
                EstaActivo = true,
                UltimaActualizacion = DateTime.UtcNow
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

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

        public async Task<ProductoResponse> UpdateProductoAsync(int id, UpdateProductoRequest request)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

            producto.Nombre = request.Nombre;
            producto.PrecioPorLibra = request.PrecioPorLibra;
            producto.TipoEmpaque = request.TipoEmpaque;
            producto.UltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
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

        public async Task<bool> UpdateProductoStatusAsync(int id, bool isActive)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

            producto.EstaActivo = isActive;
            producto.UltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            InvalidateCache();

            return true;
        }

        public async Task<bool> UpdateProductoStockAsync(int productoId, decimal cantidadLibras, bool isAddition)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            if (isAddition)
            {
                producto.CantidadLibras += cantidadLibras;
            }
            else
            {
                if (producto.CantidadLibras < cantidadLibras)
                    throw new InvalidOperationException($"Stock insuficiente para el producto {producto.Nombre}");

                producto.CantidadLibras -= cantidadLibras;
            }

            producto.UltimaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            InvalidateCache();

            return true;
        }

        private void InvalidateCache()
        {
            _cache.Remove(ALL_PRODUCTS_CACHE_KEY);
            _cache.Remove(ACTIVE_PRODUCTS_CACHE_KEY);
        }
    }
} 