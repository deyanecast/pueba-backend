using Microsoft.EntityFrameworkCore;
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

        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductoResponse> CreateProductoAsync(CreateProductoRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
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

                return MapProductoToResponse(producto);
            });
        }

        public async Task<List<ProductoResponse>> GetProductosAsync()
        {
            var productos = await _context.Productos
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return productos.Select(MapProductoToResponse).ToList();
        }

        public async Task<ProductoResponse> GetProductoByIdAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

            return MapProductoToResponse(producto);
        }

        public async Task<List<ProductoResponse>> GetActiveProductosAsync()
        {
            var productos = await _context.Productos
                .Where(p => p.EstaActivo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return productos.Select(MapProductoToResponse).ToList();
        }

        public async Task<ProductoResponse> UpdateProductoAsync(int id, UpdateProductoRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

                producto.Nombre = request.Nombre;
                producto.PrecioPorLibra = request.PrecioPorLibra;
                producto.TipoEmpaque = request.TipoEmpaque;
                producto.UltimaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return MapProductoToResponse(producto);
            });
        }

        public async Task<bool> UpdateProductoStatusAsync(int id, bool isActive)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

                producto.EstaActivo = isActive;
                producto.UltimaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            });
        }

        public async Task<bool> ValidateProductoStockAsync(int productoId, decimal cantidadLibras)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            return producto.EstaActivo && producto.CantidadLibras >= cantidadLibras;
        }

        public async Task<bool> UpdateProductoStockAsync(int productoId, decimal cantidadLibras, bool isAddition)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
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
                return true;
            });
        }

        private static ProductoResponse MapProductoToResponse(Producto producto)
        {
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
    }
} 