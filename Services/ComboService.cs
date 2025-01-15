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
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Verificar productos y cargarlos en memoria
                    var productosIds = request.Productos.Select(p => p.ProductoId).Distinct().ToList();
                    var productos = await _context.Productos
                        .AsNoTracking()
                        .Where(p => productosIds.Contains(p.ProductoId))
                        .ToDictionaryAsync(p => p.ProductoId, p => p);

                    // Verificar que todos los productos existan
                    var productosNoEncontrados = productosIds
                        .Where(id => !productos.ContainsKey(id))
                        .ToList();

                    if (productosNoEncontrados.Any())
                    {
                        throw new KeyNotFoundException(
                            $"Los siguientes productos no fueron encontrados: {string.Join(", ", productosNoEncontrados)}");
                    }

                    // Crear el combo
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

                    // Crear los detalles del combo
                    var detalles = request.Productos.Select(detalle => new ComboDetalle
                    {
                        ComboId = combo.ComboId,
                        ProductoId = detalle.ProductoId,
                        CantidadLibras = detalle.CantidadLibras
                    }).ToList();

                    // Establecer el estado de los detalles como Added
                    foreach (var detalle in detalles)
                    {
                        _context.Entry(detalle).State = EntityState.Added;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Cargar el combo completo
                    return await GetComboByIdAsync(combo.ComboId);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
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
                    Productos = c.ComboDetalles
                        .Where(cd => cd.Producto != null)
                        .Select(cd => new ComboDetalleResponse
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
                    Productos = c.ComboDetalles
                        .Where(cd => cd.Producto != null)
                        .Select(cd => new ComboDetalleResponse
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
                    Productos = c.ComboDetalles
                        .Where(cd => cd.Producto != null)
                        .Select(cd => new ComboDetalleResponse
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