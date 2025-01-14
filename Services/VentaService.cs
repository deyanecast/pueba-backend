using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductoService _productoService;
        private readonly IComboService _comboService;

        public VentaService(
            ApplicationDbContext context,
            IProductoService productoService,
            IComboService comboService)
        {
            _context = context;
            _productoService = productoService;
            _comboService = comboService;
        }

        public async Task<VentaResponse> CreateVentaAsync(CreateVentaRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Crear la venta
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

                    // Procesar los detalles
                    decimal montoTotal = 0;
                    var detalles = new List<VentaDetalle>();

                    foreach (var detalle in request.Detalles)
                    {
                        var ventaDetalle = new VentaDetalle
                        {
                            VentaId = venta.VentaId,
                            TipoItem = detalle.TipoItem,
                            ProductoId = detalle.ProductoId,
                            ComboId = detalle.ComboId,
                            CantidadLibras = detalle.CantidadLibras
                        };

                        if (detalle.TipoItem == "PRODUCTO")
                        {
                            if (!await _productoService.ValidateProductoStockAsync(detalle.ProductoId.Value, detalle.CantidadLibras))
                                throw new InvalidOperationException($"Stock insuficiente para el producto {detalle.ProductoId}");

                            var producto = await _context.Productos.FindAsync(detalle.ProductoId.Value);
                            ventaDetalle.PrecioUnitario = producto.PrecioPorLibra;
                            ventaDetalle.Subtotal = detalle.CantidadLibras * producto.PrecioPorLibra;

                            await _productoService.UpdateProductoStockAsync(detalle.ProductoId.Value, detalle.CantidadLibras, false);
                        }
                        else if (detalle.TipoItem == "COMBO")
                        {
                            if (!await _comboService.ValidateComboStockAsync(detalle.ComboId.Value, detalle.CantidadLibras))
                                throw new InvalidOperationException($"Stock insuficiente para el combo {detalle.ComboId}");

                            var combo = await _context.Combos.FindAsync(detalle.ComboId.Value);
                            ventaDetalle.PrecioUnitario = combo.Precio;
                            ventaDetalle.Subtotal = combo.Precio * detalle.CantidadLibras;

                            var comboDetalles = await _context.ComboDetalles
                                .Where(cd => cd.ComboId == detalle.ComboId.Value)
                                .ToListAsync();

                            foreach (var comboDetalle in comboDetalles)
                            {
                                await _productoService.UpdateProductoStockAsync(
                                    comboDetalle.ProductoId,
                                    comboDetalle.CantidadLibras * detalle.CantidadLibras,
                                    false);
                            }
                        }

                        detalles.Add(ventaDetalle);
                        montoTotal += ventaDetalle.Subtotal;
                    }

                    // Guardar los detalles
                    await _context.VentaDetalles.AddRangeAsync(detalles);
                    
                    // Actualizar el monto total
                    venta.MontoTotal = montoTotal;
                    
                    // Guardar todos los cambios
                    await _context.SaveChangesAsync();

                    // Confirmar la transacción
                    await transaction.CommitAsync();

                    // Construir la respuesta directamente con los datos que ya tenemos
                    return new VentaResponse
                    {
                        VentaId = venta.VentaId,
                        Cliente = venta.Cliente,
                        Observaciones = venta.Observaciones,
                        FechaVenta = venta.FechaVenta,
                        MontoTotal = venta.MontoTotal,
                        TipoVenta = venta.TipoVenta,
                        Detalles = detalles.Select(d => new VentaDetalleResponse
                        {
                            DetalleVentaId = d.DetalleVentaId,
                            TipoItem = d.TipoItem,
                            CantidadLibras = d.CantidadLibras,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<VentaResponse>> GetVentasAsync()
        {
            var ventas = await _context.Ventas
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Producto)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Combo)
                        .ThenInclude(c => c.ComboDetalles)
                            .ThenInclude(cd => cd.Producto)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            return ventas.Select(MapVentaToResponse).ToList();
        }

        public async Task<VentaResponse> GetVentaByIdAsync(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Producto)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Combo)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VentaId == id);

            if (venta == null)
                throw new KeyNotFoundException($"Venta con ID {id} no encontrada");

            var detallesConCombo = venta.VentaDetalles.Where(vd => vd.ComboId != null).ToList();
            if (detallesConCombo.Any())
            {
                var comboIds = detallesConCombo.Select(vd => vd.ComboId.Value).Distinct().ToList();
                var combosConDetalles = await _context.Combos
                    .Include(c => c.ComboDetalles)
                        .ThenInclude(cd => cd.Producto)
                    .Where(c => comboIds.Contains(c.ComboId))
                    .AsSplitQuery()
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var detalle in detallesConCombo)
                {
                    detalle.Combo = combosConDetalles.FirstOrDefault(c => c.ComboId == detalle.ComboId);
                }
            }

            return MapVentaToResponse(venta);
        }

        public async Task<List<VentaResponse>> GetVentasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var ventas = await _context.Ventas
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Producto)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Combo)
                .Where(v => v.FechaVenta >= startDate && v.FechaVenta <= endDate)
                .OrderByDescending(v => v.FechaVenta)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            var ventasConCombos = ventas.Where(v => v.VentaDetalles.Any(vd => vd.ComboId != null)).ToList();
            if (ventasConCombos.Any())
            {
                var comboIds = ventasConCombos
                    .SelectMany(v => v.VentaDetalles)
                    .Where(vd => vd.ComboId != null)
                    .Select(vd => vd.ComboId.Value)
                    .Distinct()
                    .ToList();

                var combosConDetalles = await _context.Combos
                    .Include(c => c.ComboDetalles)
                        .ThenInclude(cd => cd.Producto)
                    .Where(c => comboIds.Contains(c.ComboId))
                    .AsSplitQuery()
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var venta in ventasConCombos)
                {
                    foreach (var detalle in venta.VentaDetalles.Where(vd => vd.ComboId != null))
                    {
                        detalle.Combo = combosConDetalles.FirstOrDefault(c => c.ComboId == detalle.ComboId);
                    }
                }
            }

            return ventas.Select(MapVentaToResponse).ToList();
        }

        public async Task<decimal> GetTotalVentasByDateAsync(DateTime date)
        {
            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1).AddTicks(-1);

            return await _context.Ventas
                .Where(v => v.FechaVenta >= startDate && v.FechaVenta <= endDate)
                .AsNoTracking()
                .SumAsync(v => v.MontoTotal);
        }

        private static VentaResponse MapVentaToResponse(Venta venta)
        {
            return new VentaResponse
            {
                VentaId = venta.VentaId,
                Cliente = venta.Cliente,
                Observaciones = venta.Observaciones,
                FechaVenta = venta.FechaVenta,
                MontoTotal = venta.MontoTotal,
                TipoVenta = venta.TipoVenta,
                Detalles = venta.VentaDetalles.Select(d => new VentaDetalleResponse
                {
                    DetalleVentaId = d.DetalleVentaId,
                    TipoItem = d.TipoItem,
                    Producto = d.Producto != null ? MapProductoToResponse(d.Producto) : null,
                    Combo = d.Combo != null ? MapComboToResponse(d.Combo) : null,
                    CantidadLibras = d.CantidadLibras,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
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
    }
} 