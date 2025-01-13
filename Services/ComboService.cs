using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Repositories;
using MiBackend.Interfaces.Services;
using MiBackend.Models;

namespace MiBackend.Services;

public class ComboService : IComboService
{
    private readonly IGenericRepository<Combo> _comboRepository;
    private readonly IGenericRepository<Producto> _productoRepository;
    private readonly ILogger<ComboService> _logger;

    public ComboService(
        IGenericRepository<Combo> comboRepository,
        IGenericRepository<Producto> productoRepository,
        ILogger<ComboService> logger)
    {
        _comboRepository = comboRepository;
        _productoRepository = productoRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ComboResponse>> GetAllAsync()
    {
        var combos = await _comboRepository.GetAllAsync();
        var responses = new List<ComboResponse>();
        
        foreach (var combo in combos)
        {
            responses.Add(await MapToResponse(combo));
        }
        
        return responses;
    }

    public async Task<ComboResponse?> GetByIdAsync(int id)
    {
        var combo = await _comboRepository.GetByIdAsync(id);
        return combo != null ? await MapToResponse(combo) : null;
    }

    public async Task<ComboResponse> CreateAsync(CreateComboRequest request)
    {
        // Verificar que todos los productos existan y estén activos
        foreach (var producto in request.Productos)
        {
            var productoExistente = await _productoRepository.GetByIdAsync(producto.ProductoId);
            if (productoExistente == null)
            {
                throw new InvalidOperationException($"El producto con ID {producto.ProductoId} no existe");
            }
            if (!productoExistente.EstaActivo)
            {
                throw new InvalidOperationException($"El producto {productoExistente.Nombre} no está activo");
            }
        }

        var combo = new Combo
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            EstaActivo = true,
            UltimaActualizacion = DateTime.UtcNow,
            Productos = request.Productos.Select(p => new ComboDetalle
            {
                ProductoId = p.ProductoId,
                CantidadLibras = p.Cantidad
            }).ToList()
        };

        var createdCombo = await _comboRepository.CreateAsync(combo);
        _logger.LogInformation("Combo creado exitosamente: {ComboId}", createdCombo.ComboId);
        return await MapToResponse(createdCombo);
    }

    public async Task<ComboResponse> UpdateAsync(int id, CreateComboRequest request)
    {
        var combo = await _comboRepository.GetByIdAsync(id);
        if (combo == null)
        {
            throw new KeyNotFoundException($"Combo con ID {id} no encontrado");
        }

        combo.Nombre = request.Nombre;
        combo.Descripcion = request.Descripcion;
        combo.Precio = request.Precio;
        combo.UltimaActualizacion = DateTime.UtcNow;
        combo.Productos = request.Productos.Select(p => new ComboDetalle
        {
            ProductoId = p.ProductoId,
            CantidadLibras = p.Cantidad
        }).ToList();

        var updatedCombo = await _comboRepository.UpdateAsync(combo);
        return await MapToResponse(updatedCombo);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _comboRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new KeyNotFoundException($"Combo con ID {id} no encontrado");
        }

        await _comboRepository.DeleteAsync(id);
    }

    public async Task<ComboResponse> ToggleEstadoAsync(int id)
    {
        var combo = await _comboRepository.GetByIdAsync(id);
        if (combo == null)
        {
            throw new KeyNotFoundException($"Combo con ID {id} no encontrado");
        }

        combo.EstaActivo = !combo.EstaActivo;
        combo.UltimaActualizacion = DateTime.UtcNow;

        var updatedCombo = await _comboRepository.UpdateAsync(combo);
        return await MapToResponse(updatedCombo);
    }

    private async Task<ComboResponse> MapToResponse(Combo combo)
    {
        var productosResponse = new List<ComboProductoResponse>();
        foreach (var comboDetalle in combo.Productos)
        {
            var producto = await _productoRepository.GetByIdAsync(comboDetalle.ProductoId);
            if (producto != null)
            {
                productosResponse.Add(new ComboProductoResponse
                {
                    ProductoId = producto.ProductoId,
                    NombreProducto = producto.Nombre,
                    Cantidad = (int)comboDetalle.CantidadLibras,
                    PrecioUnitario = producto.PrecioPorLibra
                });
            }
        }

        return new ComboResponse
        {
            ComboId = combo.ComboId,
            Nombre = combo.Nombre,
            Descripcion = combo.Descripcion,
            Precio = combo.Precio,
            EstaActivo = combo.EstaActivo,
            Productos = productosResponse,
            UltimaActualizacion = combo.UltimaActualizacion
        };
    }
} 