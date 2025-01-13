using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Repositories;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Services;

public class VentaService : IVentaService
{
    private readonly IGenericRepository<Venta> _ventaRepository;
    private readonly IGenericRepository<Producto> _productoRepository;
    private readonly IGenericRepository<Combo> _comboRepository;
    private readonly ILogger<VentaService> _logger;

    public VentaService(
        IGenericRepository<Venta> ventaRepository,
        IGenericRepository<Producto> productoRepository,
        IGenericRepository<Combo> comboRepository,
        ILogger<VentaService> logger)
    {
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _comboRepository = comboRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<VentaResponse>> GetAllAsync()
    {
        var ventas = await _ventaRepository.GetAllAsync();
        return await Task.WhenAll(ventas.Select(MapToResponse));
    }

    public async Task<VentaResponse?> GetByIdAsync(int id)
    {
        var venta = await _ventaRepository.GetByIdAsync(id);
        return venta != null ? await MapToResponse(venta) : null;
    }

    public async Task<VentaResponse> CreateAsync(CreateVentaRequest request)
    {
        var items = new List<VentaItem>();

        foreach (var item in request.Items)
        {
            decimal precioUnitario;

            if (item.TipoItem.Equals("Producto", StringComparison.OrdinalIgnoreCase))
            {
                var producto = await _productoRepository.GetByIdAsync(item.ItemId);
                if (producto == null)
                {
                    throw new InvalidOperationException($"El producto con ID {item.ItemId} no existe");
                }
                if (!producto.EstaActivo)
                {
                    throw new InvalidOperationException($"El producto {producto.Nombre} no está activo");
                }
                precioUnitario = producto.PrecioPorLibra;
            }
            else if (item.TipoItem.Equals("Combo", StringComparison.OrdinalIgnoreCase))
            {
                var combo = await _comboRepository.GetByIdAsync(item.ItemId);
                if (combo == null)
                {
                    throw new InvalidOperationException($"El combo con ID {item.ItemId} no existe");
                }
                if (!combo.EstaActivo)
                {
                    throw new InvalidOperationException($"El combo {combo.Nombre} no está activo");
                }
                precioUnitario = combo.Precio;
            }
            else
            {
                throw new InvalidOperationException($"Tipo de ítem no válido: {item.TipoItem}");
            }

            items.Add(new VentaItem
            {
                TipoItem = item.TipoItem,
                ItemId = item.ItemId,
                Cantidad = item.Cantidad,
                PrecioUnitario = precioUnitario
            });
        }

        var venta = new Venta
        {
            Cliente = request.Cliente,
            Observaciones = request.Observaciones,
            FechaVenta = DateTime.UtcNow,
            Items = items
        };

        var createdVenta = await _ventaRepository.CreateAsync(venta);
        _logger.LogInformation("Venta creada exitosamente: {VentaId}", createdVenta.VentaId);
        return await MapToResponse(createdVenta);
    }

    public async Task<object> GenerarReporteAsync(DateTime? fechaInicio, DateTime? fechaFin, string? cliente)
    {
        var ventas = await _ventaRepository.GetAllAsync();
        var query = ventas.AsQueryable();

        if (fechaInicio.HasValue)
        {
            query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(v => v.FechaVenta <= fechaFin.Value);
        }

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            query = query.Where(v => v.Cliente.Contains(cliente, StringComparison.OrdinalIgnoreCase));
        }

        var ventasFiltradas = await Task.WhenAll(query.Select(MapToResponse));

        return new
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Cliente = cliente,
            TotalVentas = ventasFiltradas.Count(),
            MontoTotal = ventasFiltradas.Sum(v => v.Total),
            Ventas = ventasFiltradas
        };
    }

    private async Task<VentaResponse> MapToResponse(Venta venta)
    {
        var itemsResponse = new List<VentaItemResponse>();

        foreach (var item in venta.Items)
        {
            string nombre;
            if (item.TipoItem.Equals("Producto", StringComparison.OrdinalIgnoreCase))
            {
                var producto = await _productoRepository.GetByIdAsync(item.ItemId);
                nombre = producto?.Nombre ?? "Producto no encontrado";
            }
            else
            {
                var combo = await _comboRepository.GetByIdAsync(item.ItemId);
                nombre = combo?.Nombre ?? "Combo no encontrado";
            }

            itemsResponse.Add(new VentaItemResponse
            {
                TipoItem = item.TipoItem,
                ItemId = item.ItemId,
                Nombre = nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            });
        }

        return new VentaResponse
        {
            VentaId = venta.VentaId,
            Cliente = venta.Cliente,
            Items = itemsResponse,
            Observaciones = venta.Observaciones,
            FechaVenta = venta.FechaVenta
        };
    }
} 