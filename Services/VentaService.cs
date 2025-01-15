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
                            Venta = venta,
                            TipoItem = detalle.TipoItem,
                            ProductoId = detalle.ProductoId,
                            ComboId = detalle.ComboId,
                            CantidadLibras = detalle.CantidadLibras
                        };

                        if (detalle.TipoItem == "PRODUCTO")
                        {
                            if (detalle.ProductoId == null)
                                throw new InvalidOperationException("ProductoId es requerido para items de tipo PRODUCTO");

                            if (!await _productoService.ValidateProductoStockAsync(detalle.ProductoId.Value, detalle.CantidadLibras))
                                throw new InvalidOperationException($"Stock insuficiente para el producto {detalle.ProductoId}");

                            var producto = await _context.Productos.FindAsync(detalle.ProductoId.Value);
                            if (producto == null)
                                throw new InvalidOperationException($"Producto con ID {detalle.ProductoId} no encontrado");

                            ventaDetalle.PrecioUnitario = producto.PrecioPorLibra;
                            ventaDetalle.Subtotal = detalle.CantidadLibras * producto.PrecioPorLibra;

                            await _productoService.UpdateProductoStockAsync(detalle.ProductoId.Value, detalle.CantidadLibras, false);
                        }
                        else if (detalle.TipoItem == "COMBO")
                        {
                            if (detalle.ComboId == null)
                                throw new InvalidOperationException("ComboId es requerido para items de tipo COMBO");

                            if (!await _comboService.ValidateComboStockAsync(detalle.ComboId.Value, detalle.CantidadLibras))
                                throw new InvalidOperationException($"Stock insuficiente para el combo {detalle.ComboId}");

                            var combo = await _context.Combos.FindAsync(detalle.ComboId.Value);
                            if (combo == null)
                                throw new InvalidOperationException($"Combo con ID {detalle.ComboId} no encontrado");

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

                    // Cargar los datos relacionados necesarios para la respuesta
                    foreach (var detalle in detalles)
                    {
                        if (detalle.ProductoId.HasValue)
                        {
                            await _context.Entry(detalle)
                                .Reference(d => d.Producto)
                                .LoadAsync();
                        }
                        if (detalle.ComboId.HasValue)
                        {
                            await _context.Entry(detalle)
                                .Reference(d => d.Combo)
                                .LoadAsync();
                        }
                    }

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
                            Subtotal = d.Subtotal,
                            Producto = d.ProductoId.HasValue && d.Producto != null ? new ProductoResponse
                            {
                                ProductoId = d.Producto.ProductoId,
                                Nombre = d.Producto.Nombre,
                                CantidadLibras = d.Producto.CantidadLibras,
                                PrecioPorLibra = d.Producto.PrecioPorLibra,
                                TipoEmpaque = d.Producto.TipoEmpaque,
                                EstaActivo = d.Producto.EstaActivo,
                                UltimaActualizacion = d.Producto.UltimaActualizacion
                            } : null,
                            Combo = d.ComboId.HasValue && d.Combo != null ? new ComboResponse
                            {
                                ComboId = d.Combo.ComboId,
                                Nombre = d.Combo.Nombre,
                                Descripcion = d.Combo.Descripcion,
                                Precio = d.Combo.Precio,
                                EstaActivo = d.Combo.EstaActivo,
                                UltimaActualizacion = d.Combo.UltimaActualizacion,
                                Productos = d.Combo.ComboDetalles != null ? d.Combo.ComboDetalles.Select(cd => new ComboDetalleResponse
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
                                }).ToList() : new List<ComboDetalleResponse>()
                            } : null
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
                        Producto = d.Producto != null ? new ProductoResponse
                        {
                            ProductoId = d.Producto.ProductoId,
                            Nombre = d.Producto.Nombre,
                            CantidadLibras = d.Producto.CantidadLibras,
                            PrecioPorLibra = d.Producto.PrecioPorLibra,
                            TipoEmpaque = d.Producto.TipoEmpaque,
                            EstaActivo = d.Producto.EstaActivo,
                            UltimaActualizacion = d.Producto.UltimaActualizacion
                        } : null,
                        Combo = d.Combo != null ? new ComboResponse
                        {
                            ComboId = d.Combo.ComboId,
                            Nombre = d.Combo.Nombre,
                            Descripcion = d.Combo.Descripcion,
                            Precio = d.Combo.Precio,
                            EstaActivo = d.Combo.EstaActivo,
                            UltimaActualizacion = d.Combo.UltimaActualizacion,
                            Productos = d.Combo.ComboDetalles != null ? d.Combo.ComboDetalles.Select(cd => new ComboDetalleResponse
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
                            }).ToList() : new List<ComboDetalleResponse>()
                        } : null
                    }).ToList()
                })
                .OrderByDescending(v => v.FechaVenta)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            return ventas;
        }

        public async Task<VentaResponse> GetVentaByIdAsync(int id)
        {
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
                        Producto = d.Producto != null ? new ProductoResponse
                        {
                            ProductoId = d.Producto.ProductoId,
                            Nombre = d.Producto.Nombre,
                            CantidadLibras = d.Producto.CantidadLibras,
                            PrecioPorLibra = d.Producto.PrecioPorLibra,
                            TipoEmpaque = d.Producto.TipoEmpaque,
                            EstaActivo = d.Producto.EstaActivo,
                            UltimaActualizacion = d.Producto.UltimaActualizacion
                        } : null,
                        Combo = d.Combo != null ? new ComboResponse
                        {
                            ComboId = d.Combo.ComboId,
                            Nombre = d.Combo.Nombre,
                            Descripcion = d.Combo.Descripcion,
                            Precio = d.Combo.Precio,
                            EstaActivo = d.Combo.EstaActivo,
                            UltimaActualizacion = d.Combo.UltimaActualizacion,
                            Productos = d.Combo.ComboDetalles != null ? d.Combo.ComboDetalles.Select(cd => new ComboDetalleResponse
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
                            }).ToList() : new List<ComboDetalleResponse>()
                        } : null
                    }).ToList()
                })
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VentaId == id);

            if (venta == null)
                throw new KeyNotFoundException($"Venta con ID {id} no encontrada");

            return venta;
        }

        public async Task<List<VentaResponse>> GetVentasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

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
                        Producto = d.Producto != null ? new ProductoResponse
                        {
                            ProductoId = d.Producto.ProductoId,
                            Nombre = d.Producto.Nombre,
                            CantidadLibras = d.Producto.CantidadLibras,
                            PrecioPorLibra = d.Producto.PrecioPorLibra,
                            TipoEmpaque = d.Producto.TipoEmpaque,
                            EstaActivo = d.Producto.EstaActivo,
                            UltimaActualizacion = d.Producto.UltimaActualizacion
                        } : null,
                        Combo = d.Combo != null ? new ComboResponse
                        {
                            ComboId = d.Combo.ComboId,
                            Nombre = d.Combo.Nombre,
                            Descripcion = d.Combo.Descripcion,
                            Precio = d.Combo.Precio,
                            EstaActivo = d.Combo.EstaActivo,
                            UltimaActualizacion = d.Combo.UltimaActualizacion,
                            Productos = d.Combo.ComboDetalles != null ? d.Combo.ComboDetalles.Select(cd => new ComboDetalleResponse
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
                            }).ToList() : new List<ComboDetalleResponse>()
                        } : null
                    }).ToList()
                })
                .OrderByDescending(v => v.FechaVenta)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            return ventas;
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
    }
} 