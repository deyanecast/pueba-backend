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
                    UltimaActualizacion = DateTime.UtcNow,
                    ComboDetalles = request.Productos.Select(p => new ComboDetalle
                    {
                        ProductoId = p.ProductoId,
                        CantidadLibras = p.CantidadLibras
                    }).ToList()
                };

                _context.Combos.Add(combo);
                await _context.SaveChangesAsync();

                return await GetComboByIdAsync(combo.ComboId);
            });
        }

        public async Task<List<ComboResponse>> GetCombosAsync()
        {
            var combos = await _context.Combos
                .Include(c => c.ComboDetalles)
                    .ThenInclude(cd => cd.Producto)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return combos.Select(MapComboToResponse).ToList();
        }

        public async Task<ComboResponse> GetComboByIdAsync(int id)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboDetalles)
                    .ThenInclude(cd => cd.Producto)
                .FirstOrDefaultAsync(c => c.ComboId == id);

            if (combo == null)
                throw new KeyNotFoundException($"Combo con ID {id} no encontrado");

            return MapComboToResponse(combo);
        }

        public async Task<List<ComboResponse>> GetActiveCombosAsync()
        {
            var combos = await _context.Combos
                .Include(c => c.ComboDetalles)
                    .ThenInclude(cd => cd.Producto)
                .Where(c => c.EstaActivo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return combos.Select(MapComboToResponse).ToList();
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

        private static ComboResponse MapComboToResponse(Combo combo)
        {
            return new ComboResponse
            {
                ComboId = combo.ComboId,
                Nombre = combo.Nombre,
                Descripcion = combo.Descripcion,
                Precio = combo.Precio,
                EstaActivo = combo.EstaActivo,
                UltimaActualizacion = combo.UltimaActualizacion,
                Productos = combo.ComboDetalles.Select(cd => new ComboDetalleResponse
                {
                    ComboDetalleId = cd.ComboDetalleId,
                    Producto = MapProductoToResponse(cd.Producto),
                    CantidadLibras = cd.CantidadLibras
                }).ToList()
            };
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