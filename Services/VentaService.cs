using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Extensions;
using MiBackend.Interfaces;
using MiBackend.Interfaces.Services;
using MiBackend.Models;
using Microsoft.Extensions.DependencyInjection;
using MiBackend.Strategies;
using MiBackend.Helpers;
using Microsoft.Extensions.Logging;

namespace MiBackend.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VentaService> _logger;

        public VentaService(ApplicationDbContext context, IMemoryCache cache, IUnitOfWork unitOfWork, IServiceProvider serviceProvider, ILogger<VentaService> logger)
        {
            _context = context;
            _cache = cache;
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<VentaResponse?> GetVentaByIdAsync(int id)
        {
            try
            {
                string cacheKey = $"venta_{id}";

                if (_cache.TryGetValue(cacheKey, out VentaResponse cachedVenta))
                    return cachedVenta;

                var venta = await _context.Ventas
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.Producto)
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(vd => vd.Combo)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(v => v.VentaId == id);

                if (venta == null)
                    return null;

                var ventaResponse = new VentaResponse
                {
                    VentaId = venta.VentaId,
                    Cliente = venta.Cliente,
                    Observaciones = venta.Observaciones,
                    TipoVenta = venta.TipoVenta,
                    FechaVenta = venta.FechaVenta,
                    Total = venta.Total,
                    Detalles = venta.VentaDetalles.Select(vd => new VentaDetalleResponse
                    {
                        VentaDetalleId = vd.VentaDetalleId,
                        TipoItem = vd.TipoItem,
                        Cantidad = vd.CantidadLibras,
                        Total = vd.Subtotal
                    }).ToList()
                };

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, ventaResponse, cacheEntryOptions);

                return ventaResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta por ID {VentaId}", id);
                throw new InvalidOperationException($"Error al obtener venta: {ex.Message}", ex);
            }
        }

        public async Task<List<VentaResponse>> GetVentasByDateAsync(DateTime date)
        {
            try
            {
                string cacheKey = $"ventas_date_{date:yyyy-MM-dd}";

                if (_cache.TryGetValue(cacheKey, out List<VentaResponse> cachedVentas))
                    return cachedVentas;

                var ventasPorFecha = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == date.Date)
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => new VentaResponse
                    {
                        VentaId = v.VentaId,
                        Cliente = v.Cliente,
                        Observaciones = v.Observaciones,
                        TipoVenta = v.TipoVenta,
                        FechaVenta = v.FechaVenta,
                        Total = v.Total,
                        Detalles = v.VentaDetalles.Select(vd => new VentaDetalleResponse
                        {
                            VentaDetalleId = vd.VentaDetalleId,
                            TipoItem = vd.TipoItem,
                            Cantidad = vd.CantidadLibras,
                            Total = vd.Subtotal
                        }).ToList()
                    })
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToListAsync();

                if (!ventasPorFecha.Any())
                    return new List<VentaResponse>();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, ventasPorFecha, cacheEntryOptions);

                return ventasPorFecha;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por fecha {Date}", date);
                throw new InvalidOperationException($"Error al obtener ventas por fecha: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalVentasByDateAsync(DateTime date)
        {
            try
            {
                string cacheKey = $"total_ventas_date_{date:yyyy-MM-dd}";

                if (_cache.TryGetValue(cacheKey, out decimal cachedTotal))
                    return cachedTotal;

                var total = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == date.Date)
                    .AsNoTracking()
                    .SumAsync(v => v.Total);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, total, cacheEntryOptions);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de ventas por fecha {Date}", date);
                throw new InvalidOperationException($"Error al obtener total de ventas: {ex.Message}", ex);
            }
        }

        public async Task<List<VentaResponse>> GetVentasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                string cacheKey = $"ventas_range_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}";

                if (_cache.TryGetValue(cacheKey, out List<VentaResponse> cachedVentas))
                    return cachedVentas;

                var startDateTime = startDate.ToStartOfDay();
                var endDateTime = endDate.ToEndOfDay();

                var ventasPorRango = await _context.Ventas
                    .Where(v => v.FechaVenta >= startDateTime && 
                           v.FechaVenta <= endDateTime)
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => new VentaResponse
                    {
                        VentaId = v.VentaId,
                        Cliente = v.Cliente,
                        Observaciones = v.Observaciones,
                        TipoVenta = v.TipoVenta,
                        FechaVenta = v.FechaVenta,
                        Total = v.Total,
                        Detalles = v.VentaDetalles.Select(vd => new VentaDetalleResponse
                        {
                            VentaDetalleId = vd.VentaDetalleId,
                            TipoItem = vd.TipoItem,
                            Cantidad = vd.CantidadLibras,
                            Total = vd.Subtotal
                        }).ToList()
                    })
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToListAsync();

                if (!ventasPorRango.Any())
                    return new List<VentaResponse>();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, ventasPorRango, cacheEntryOptions);

                return ventasPorRango;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener ventas por rango de fechas: {ex.Message}", ex);
            }
        }

        public async Task<VentaResponse> CreateVentaAsync(CreateVentaRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var venta = new Venta
                {
                    Cliente = request.Cliente,
                    Observaciones = request.Observaciones,
                    TipoVenta = request.TipoVenta,
                    FechaVenta = DateTime.UtcNow,
                    Total = 0
                };

                await _unitOfWork.Repository<Venta>().AddAsync(venta);
                await _unitOfWork.SaveChangesAsync();

                decimal totalVenta = 0;
                var ventaDetalles = new List<VentaDetalle>();

                foreach (var detalle in request.Detalles)
                {
                    IVentaItemStrategy strategy = detalle.TipoItem switch
                    {
                        "PRODUCTO" => _serviceProvider.GetRequiredService<ProductoVentaStrategy>(),
                        "COMBO" => _serviceProvider.GetRequiredService<ComboVentaStrategy>(),
                        _ => throw new InvalidOperationException($"Tipo de item no válido: {detalle.TipoItem}")
                    };

                    var ventaDetalle = await strategy.ProcessVentaDetalleAsync(venta, detalle);
                    ventaDetalles.Add(ventaDetalle);
                    totalVenta += ventaDetalle.Subtotal;

                    await _unitOfWork.Repository<VentaDetalle>().AddAsync(ventaDetalle);
                }

                venta.Total = totalVenta;
                venta.VentaDetalles = ventaDetalles;
                _unitOfWork.Repository<Venta>().Update(venta);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Invalidar el caché
                var cacheKeys = _cache.GetKeys<string>().Where(k => k.StartsWith("ventas_") || k.StartsWith("total_ventas_"));
                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }

                var response = await GetVentaByIdAsync(venta.VentaId);
                if (response == null)
                    throw new InvalidOperationException("No se pudo crear la venta");

                return response;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"Error al crear la venta: {ex.Message}", ex);
            }
        }

        public async Task<DashboardResponse> GetDashboardDataAsync()
        {
            try
            {
                string cacheKey = "dashboard_data";
                if (_cache.TryGetValue(cacheKey, out DashboardResponse cachedData))
                    return cachedData;

                var today = DateTime.UtcNow.Date;
                
                // Ejecutar todas las consultas en paralelo
                var ventasDelDiaTask = GetTotalVentasByDateAsync(today);
                var totalProductosActivosTask = _context.Productos
                    .AsNoTracking()
                    .CountAsync(p => p.EstaActivo);
                var productosStockBajoTask = _context.Productos
                    .Where(p => p.CantidadLibras < 10 && p.EstaActivo)
                    .Select(p => new ProductoStockBajoResponse
                    {
                        ProductoId = p.ProductoId,
                        Nombre = p.Nombre,
                        CantidadLibras = p.CantidadLibras
                    })
                    .AsNoTracking()
                    .ToListAsync();

                // Esperar a que todas las tareas terminen
                await Task.WhenAll(ventasDelDiaTask, totalProductosActivosTask, productosStockBajoTask);

                var dashboardData = new DashboardResponse
                {
                    VentasDelDia = await ventasDelDiaTask,
                    FechaActualizacion = DateTime.UtcNow,
                    TotalProductosActivos = await totalProductosActivosTask,
                    ProductosStockBajo = await productosStockBajoTask
                };

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, dashboardData, cacheEntryOptions);

                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del dashboard");
                throw new InvalidOperationException($"Error al obtener datos del dashboard: {ex.Message}", ex);
            }
        }
    }
} 