using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;

namespace MiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;

    public VentasController(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VentaResponse>> CreateVenta([FromBody] CreateVentaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _ventaService.CreateVentaAsync(request);
            return CreatedAtAction(nameof(GetVentaById), new { id = response.VentaId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la venta", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<VentaResponse>>> GetVentas()
    {
        try
        {
            var ventas = await _ventaService.GetVentasAsync();
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las ventas", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VentaResponse>> GetVentaById(int id)
    {
        try
        {
            var venta = await _ventaService.GetVentaByIdAsync(id);
            return Ok(venta);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener la venta", error = ex.Message });
        }
    }

    [HttpGet("range")]
    public async Task<ActionResult<List<VentaResponse>>> GetVentasByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var ventas = await _ventaService.GetVentasByDateRangeAsync(startDate, endDate);
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las ventas por rango de fecha", error = ex.Message });
        }
    }

    [HttpGet("total/date")]
    public async Task<ActionResult<decimal>> GetTotalVentasByDate([FromQuery] DateTime date)
    {
        try
        {
            var total = await _ventaService.GetTotalVentasByDateAsync(date);
            return Ok(new { date = date.Date, total });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el total de ventas", error = ex.Message });
        }
    }
} 