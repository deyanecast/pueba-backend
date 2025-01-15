using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;
using MiBackend.Models;
using MiBackend.Strategies;
using Microsoft.Extensions.Logging;

namespace MiBackend.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IEnumerable<IVentaItemStrategy> _strategies;
        private readonly ILogger<VentaService> _logger;

        public VentaService(
            ApplicationDbContext context,
            IMemoryCache cache,
            IEnumerable<IVentaItemStrategy> strategies,
            ILogger<VentaService> logger)
        {
            _context = context;
            _cache = cache;
            _strategies = strategies;
            _logger = logger;
        }

        public async Task<VentaResponse> CreateVentaAsync(CreateVentaRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("Iniciando creación de venta para cliente: {Cliente}", request.Cliente);

                    var venta = new Venta
                    {
                        Cliente = request.Cliente,
                        Observaciones = request.Observaciones,
                        TipoVenta = request.TipoVenta,
                        FechaVenta = DateTime.UtcNow,
                        MontoTotal = 0
                    };

                    _context.Ventas.Add(venta);
                    await _context.SaveChangesAsync();

                    decimal montoTotal = 0;
                    var detalles = new List<VentaDetalle>();

                    foreach (var detalle in request.Detalles)
                    {
                        _logger.LogInformation("Procesando detalle de tipo: {TipoItem}", detalle.TipoItem);

                        var itemStrategy = _strategies.FirstOrDefault(s => 
                            s.GetType().Name.StartsWith(detalle.TipoItem, StringComparison.OrdinalIgnoreCase) ||
                            s.GetType().Name.Contains(detalle.TipoItem, StringComparison.OrdinalIgnoreCase))
                            ?? throw new InvalidOperationException($"Tipo de item no soportado: {detalle.TipoItem}");

                        var ventaDetalle = await itemStrategy.ProcessVentaDetalleAsync(venta, detalle);
                        detalles.Add(ventaDetalle);
                        montoTotal += ventaDetalle.Subtotal;

                        _logger.LogInformation("Detalle procesado correctamente. Subtotal: {Subtotal}", ventaDetalle.Subtotal);
                    }

                    await _context.VentaDetalles.AddRangeAsync(detalles);
                    venta.MontoTotal = montoTotal;

                    _logger.LogInformation("Guardando cambios en la base de datos...");
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Confirmando transacción...");
                    await transaction.CommitAsync();

                    // Invalidar caché relacionado
                    var cacheKeys = new[] { "AllVentas", $"Ventas_{venta.FechaVenta:yyyyMMdd}" };
                    foreach (var key in cacheKeys)
                    {
                        _cache.Remove(key);
                    }

                    _logger.LogInformation("Venta creada exitosamente con ID: {VentaId}", venta.VentaId);
                    return await GetVentaByIdAsync(venta.VentaId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al guardar cambios en la base de datos: {Message}", ex.InnerException?.Message ?? ex.Message);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Error al guardar la venta: {ex.InnerException?.Message ?? ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar la venta: {Message}", ex.Message);
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<VentaResponse>> GetVentasAsync()
        {
            const string cacheKey = "AllVentas";
            
            if (_cache.TryGetValue(cacheKey, out List<VentaResponse>? cachedVentas) && cachedVentas != null)
            {
                return cachedVentas;
            }

            var ventas = await _context.Ventas
                .Select(v => new VentaResponse
                {
                    VentaId = v.VentaId,
                    Cliente = v.Cliente,
                    Observaciones = v.Observaciones,
                    FechaVenta = v.FechaVenta,
                    MontoTotal = v.MontoTotal,
                    TipoVenta = v.TipoVenta,
                    Detalles = v.VentaDetalles.Select(d => new VentaDetalleResponse
                    {
                        DetalleVentaId = d.DetalleVentaId,
                        TipoItem = d.TipoItem,
                        CantidadLibras = d.CantidadLibras,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        Producto = d.Producto != null ? MapProductoToResponse(d.Producto) : null,
                        Combo = d.Combo != null ? MapComboToResponse(d.Combo) : null
                    }).ToList()
                })
                .OrderByDescending(v => v.FechaVenta)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(1000); // Tamaño estimado basado en la cantidad de datos
            _cache.Set(cacheKey, ventas, cacheOptions);

            return ventas;
        }

        private static ProductoResponse MapProductoToResponse(Producto producto) =>
            new()
            {
                ProductoId = producto.ProductoId,
                Nombre = producto.Nombre,
                CantidadLibras = producto.CantidadLibras,
                PrecioPorLibra = producto.PrecioPorLibra,
                TipoEmpaque = producto.TipoEmpaque,
                EstaActivo = producto.EstaActivo,
                UltimaActualizacion = producto.UltimaActualizacion
            };

        private static ComboResponse MapComboToResponse(Combo combo) =>
            new()
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

        public async Task<VentaResponse> GetVentaByIdAsync(int id)
        {
            string cacheKey = $"Venta_{id}";
            
            if (_cache.TryGetValue(cacheKey, out VentaResponse? cachedVenta) && cachedVenta != null)
            {
                return cachedVenta;
            }

            var venta = await _context.Ventas
                .Select(v => new VentaResponse
                {
                    VentaId = v.VentaId,
                    Cliente = v.Cliente,
                    Observaciones = v.Observaciones,
                    FechaVenta = v.FechaVenta,
                    MontoTotal = v.MontoTotal,
                    TipoVenta = v.TipoVenta,
                    Detalles = v.VentaDetalles.Select(d => new VentaDetalleResponse
                    {
                        DetalleVentaId = d.DetalleVentaId,
                        TipoItem = d.TipoItem,
                        CantidadLibras = d.CantidadLibras,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        Producto = d.Producto != null ? MapProductoToResponse(d.Producto) : null,
                        Combo = d.Combo != null ? MapComboToResponse(d.Combo) : null
                    }).ToList()
                })
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VentaId == id);

            if (venta == null)
                throw new KeyNotFoundException($"Venta con ID {id} no encontrada");

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(100); // Tamaño estimado para una venta individual
            _cache.Set(cacheKey, venta, cacheOptions);

            return venta;
        }

        public async Task<List<VentaResponse>> GetVentasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            string cacheKey = $"Ventas_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (_cache.TryGetValue(cacheKey, out List<VentaResponse>? cachedVentas) && cachedVentas != null)
            {
                return cachedVentas;
            }

            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta >= startDate && v.FechaVenta <= endDate)
                .Select(v => new VentaResponse
                {
                    VentaId = v.VentaId,
                    Cliente = v.Cliente,
                    Observaciones = v.Observaciones,
                    FechaVenta = v.FechaVenta,
                    MontoTotal = v.MontoTotal,
                    TipoVenta = v.TipoVenta,
                    Detalles = v.VentaDetalles.Select(d => new VentaDetalleResponse
                    {
                        DetalleVentaId = d.DetalleVentaId,
                        TipoItem = d.TipoItem,
                        CantidadLibras = d.CantidadLibras,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        Producto = d.Producto != null ? MapProductoToResponse(d.Producto) : null,
                        Combo = d.Combo != null ? MapComboToResponse(d.Combo) : null
                    }).ToList()
                })
                .OrderByDescending(v => v.FechaVenta)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetSize(500); // Tamaño estimado para un rango de ventas
            _cache.Set(cacheKey, ventas, cacheOptions);

            return ventas;
        }

        public async Task<decimal> GetTotalVentasByDateAsync(DateTime date)
        {
            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1).AddTicks(-1);
            
            string cacheKey = $"TotalVentas_{startDate:yyyyMMdd}";
            
            try 
            {
                if (_cache.TryGetValue(cacheKey, out decimal cachedTotal))
                    return cachedTotal;

                var total = await _context.Ventas
                    .Where(v => v.FechaVenta >= startDate && v.FechaVenta <= endDate)
                    .Select(v => v.MontoTotal)
                    .DefaultIfEmpty()
                    .SumAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1); // Establecemos un tamaño fijo pequeño ya que solo es un decimal

                _cache.Set(cacheKey, total, cacheOptions);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el total de ventas para la fecha {Date}", date);
                throw new InvalidOperationException("Error al obtener el total de ventas", ex);
            }
        }
    }
} 