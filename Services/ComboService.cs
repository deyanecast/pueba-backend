using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Services
{
    public class ComboService : IComboService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductoService _productoService;

        public ComboService(
            ApplicationDbContext context,
            IProductoService productoService)
        {
            _context = context;
            _productoService = productoService;
        }

        public async Task<ComboResponse> CreateComboAsync(CreateComboRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                // Validar que todos los productos existan y tengan stock suficiente
                foreach (var producto in request.Productos)
                {
                    var existingProduct = await _context.Productos.FindAsync(producto.ProductoId);
                    if (existingProduct == null)
                        throw new KeyNotFoundException($"Producto con ID {producto.ProductoId} no encontrado");

                    if (!existingProduct.EstaActivo)
                        throw new InvalidOperationException($"El producto {existingProduct.Nombre} no está activo");

                    if (!await _productoService.ValidateProductoStockAsync(producto.ProductoId, producto.CantidadLibras))
                        throw new InvalidOperationException($"Stock insuficiente para el producto {existingProduct.Nombre}");
                }

                var combo = new Combo
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Precio = request.Precio,
                    EstaActivo = true,
                    UltimaActualizacion = DateTime.UtcNow
                };

                _context.Combos.Add(combo);
                await _context.SaveChangesAsync();

                var comboDetalles = request.Productos.Select(p => new ComboDetalle
                {
                    ComboId = combo.ComboId,
                    Combo = combo,
                    ProductoId = p.ProductoId,
                    CantidadLibras = p.CantidadLibras,
                    Producto = _context.Productos.Find(p.ProductoId) ?? throw new InvalidOperationException($"Producto con ID {p.ProductoId} no encontrado")
                }).ToList();

                _context.ComboDetalles.AddRange(comboDetalles);
                await _context.SaveChangesAsync();

                return await GetComboByIdAsync(combo.ComboId);
            });
        }

        public async Task<List<ComboResponse>> GetCombosAsync()
        {
            var combos = await _context.Combos
                .Select(c => new ComboResponse
                {
                    ComboId = c.ComboId,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Precio = c.Precio,
                    EstaActivo = c.EstaActivo,
                    UltimaActualizacion = c.UltimaActualizacion,
                    Productos = c.ComboDetalles.Select(cd => new ComboDetalleResponse
                    {
                        ComboDetalleId = cd.ComboDetalleId,
                        Producto = new ProductoResponse
                        {
                            ProductoId = cd.Producto.ProductoId,
                            Nombre = cd.Producto.Nombre,
                            CantidadLibras = cd.Producto.CantidadLibras,
                            PrecioPorLibra = cd.Producto.PrecioPorLibra,
                            TipoEmpaque = cd.Producto.TipoEmpaque,
                            EstaActivo = cd.Producto.EstaActivo,
                            UltimaActualizacion = cd.Producto.UltimaActualizacion
                        },
                        CantidadLibras = cd.CantidadLibras
                    }).ToList()
                })
                .OrderBy(c => c.Nombre)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            return combos;
        }

        public async Task<ComboResponse> GetComboByIdAsync(int id)
        {
            var combo = await _context.Combos
                .Select(c => new ComboResponse
                {
                    ComboId = c.ComboId,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Precio = c.Precio,
                    EstaActivo = c.EstaActivo,
                    UltimaActualizacion = c.UltimaActualizacion,
                    Productos = c.ComboDetalles.Select(cd => new ComboDetalleResponse
                    {
                        ComboDetalleId = cd.ComboDetalleId,
                        Producto = new ProductoResponse
                        {
                            ProductoId = cd.Producto.ProductoId,
                            Nombre = cd.Producto.Nombre,
                            CantidadLibras = cd.Producto.CantidadLibras,
                            PrecioPorLibra = cd.Producto.PrecioPorLibra,
                            TipoEmpaque = cd.Producto.TipoEmpaque,
                            EstaActivo = cd.Producto.EstaActivo,
                            UltimaActualizacion = cd.Producto.UltimaActualizacion
                        },
                        CantidadLibras = cd.CantidadLibras
                    }).ToList()
                })
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ComboId == id);

            if (combo == null)
                throw new KeyNotFoundException($"Combo con ID {id} no encontrado");

            return combo;
        }

        public async Task<List<ComboResponse>> GetActiveCombosAsync()
        {
            var combos = await _context.Combos
                .Where(c => c.EstaActivo)
                .Select(c => new ComboResponse
                {
                    ComboId = c.ComboId,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Precio = c.Precio,
                    EstaActivo = c.EstaActivo,
                    UltimaActualizacion = c.UltimaActualizacion,
                    Productos = c.ComboDetalles.Select(cd => new ComboDetalleResponse
                    {
                        ComboDetalleId = cd.ComboDetalleId,
                        Producto = new ProductoResponse
                        {
                            ProductoId = cd.Producto.ProductoId,
                            Nombre = cd.Producto.Nombre,
                            CantidadLibras = cd.Producto.CantidadLibras,
                            PrecioPorLibra = cd.Producto.PrecioPorLibra,
                            TipoEmpaque = cd.Producto.TipoEmpaque,
                            EstaActivo = cd.Producto.EstaActivo,
                            UltimaActualizacion = cd.Producto.UltimaActualizacion
                        },
                        CantidadLibras = cd.CantidadLibras
                    }).ToList()
                })
                .OrderBy(c => c.Nombre)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            return combos;
        }

        public async Task<bool> UpdateComboStatusAsync(int id, bool isActive)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                var combo = await _context.Combos.FindAsync(id);
                if (combo == null)
                    throw new KeyNotFoundException($"Combo con ID {id} no encontrado");

                combo.EstaActivo = isActive;
                combo.UltimaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            });
        }

        public async Task<bool> ValidateComboStockAsync(int comboId, decimal cantidad)
        {
            var comboDetalles = await _context.ComboDetalles
                .Where(cd => cd.ComboId == comboId)
                .ToListAsync();

            foreach (var detalle in comboDetalles)
            {
                if (!await _productoService.ValidateProductoStockAsync(
                    detalle.ProductoId,
                    detalle.CantidadLibras * cantidad))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<decimal> CalculateComboTotalAsync(int comboId, decimal cantidad)
        {
            var combo = await _context.Combos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ComboId == comboId);

            if (combo == null)
                throw new KeyNotFoundException($"Combo con ID {comboId} no encontrado");

            if (!combo.EstaActivo)
                throw new InvalidOperationException($"El combo no está activo");

            return combo.Precio * cantidad;
        }
    }
} 