using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.Interfaces.Services;

namespace MiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(IProductoService productoService, ILogger<ProductosController> logger)
    {
        _productoService = productoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var productos = await _productoService.GetAllAsync();
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null)
            {
                return NotFound(new { message = $"Producto con ID {id} no encontrado" });
            }
            return Ok(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductoRequest request)
    {
        try
        {
            var producto = await _productoService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = producto.ProductoId }, producto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Intento de crear producto duplicado");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoRequest request)
    {
        try
        {
            var producto = await _productoService.UpdateAsync(id, request);
            return Ok(producto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar producto {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productoService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar producto {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPatch("{id}/toggle-estado")]
    public async Task<IActionResult> ToggleEstado(int id)
    {
        try
        {
            var producto = await _productoService.ToggleEstadoAsync(id);
            return Ok(producto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del producto {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? nombre,
        [FromQuery] decimal? precioMin,
        [FromQuery] decimal? precioMax,
        [FromQuery] bool? activo)
    {
        try
        {
            var productos = await _productoService.BuscarAsync(nombre, precioMin, precioMax, activo);
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar productos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
} 