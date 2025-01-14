using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;

namespace MiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpPost]
    public async Task<ActionResult<ProductoResponse>> CreateProducto([FromBody] CreateProductoRequest request)
    {
        try
        {
            var response = await _productoService.CreateProductoAsync(request);
            return CreatedAtAction(nameof(GetProductoById), new { id = response.ProductoId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear el producto", error = ex.Message });
        }
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
            return StatusCode(500, new { message = "Error al obtener los productos", error = ex.Message });
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
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el producto", error = ex.Message });
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
            return StatusCode(500, new { message = "Error al obtener los productos activos", error = ex.Message });
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
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar el producto", error = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateProductoStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _productoService.UpdateProductoStatusAsync(id, request.IsActive);
            return Ok(new { message = $"Estado del producto actualizado a {(request.IsActive ? "activo" : "inactivo")}" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar el estado del producto", error = ex.Message });
        }
    }

    [HttpGet("{id}/validate-stock")]
    public async Task<ActionResult<bool>> ValidateProductoStock(int id, [FromQuery] decimal cantidadLibras)
    {
        try
        {
            var isValid = await _productoService.ValidateProductoStockAsync(id, cantidadLibras);
            return Ok(new { hasStock = isValid });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al validar el stock del producto", error = ex.Message });
        }
    }

    [HttpPatch("{id}/stock")]
    public async Task<ActionResult> UpdateProductoStock(
        int id,
        [FromQuery] decimal cantidadLibras,
        [FromQuery] bool isAddition)
    {
        try
        {
            await _productoService.UpdateProductoStockAsync(id, cantidadLibras, isAddition);
            var action = isAddition ? "agregada al" : "removida del";
            return Ok(new { message = $"Cantidad {action} stock: {cantidadLibras} libras" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar el stock del producto", error = ex.Message });
        }
    }

    public class UpdateStatusRequest
    {
        public bool IsActive { get; set; }
    }
} 