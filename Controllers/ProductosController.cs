using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;
using Microsoft.Extensions.Logging;

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
    public async Task<ActionResult<List<ProductoResponse>>> GetProductos()
    {
        try
        {
            var productos = await _productoService.GetProductosAsync();
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ProductoResponse>>> GetActiveProductos()
    {
        try
        {
            var productos = await _productoService.GetActiveProductosAsync();
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos activos");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoResponse>> GetProductoById(int id)
    {
        try
        {
            var producto = await _productoService.GetProductoByIdAsync(id);
            return Ok(producto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductoResponse>> CreateProducto([FromBody] CreateProductoRequest request)
    {
        try
        {
            var producto = await _productoService.CreateProductoAsync(request);
            return Ok(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoResponse>> UpdateProducto(int id, [FromBody] UpdateProductoRequest request)
    {
        try
        {
            var producto = await _productoService.UpdateProductoAsync(id, request);
            return Ok(producto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar producto");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<bool>> UpdateProductoStatus(int id, [FromQuery] bool isActive)
    {
        try
        {
            var result = await _productoService.UpdateProductoStatusAsync(id, isActive);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado del producto");
            return StatusCode(500, new { message = ex.Message });
        }
    }
} 