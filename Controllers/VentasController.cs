using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.Interfaces.Services;

namespace MiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly ILogger<VentasController> _logger;

    public VentasController(IVentaService ventaService, ILogger<VentasController> logger)
    {
        _ventaService = ventaService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var ventas = await _ventaService.GetAllAsync();
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las ventas");
            return StatusCode(500, "Error interno del servidor al obtener las ventas");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var venta = await _ventaService.GetByIdAsync(id);
            if (venta == null)
            {
                return NotFound($"No se encontró la venta con ID {id}");
            }
            return Ok(venta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la venta {VentaId}", id);
            return StatusCode(500, "Error interno del servidor al obtener la venta");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVentaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venta = await _ventaService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = venta.VentaId }, venta);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear la venta");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la venta");
            return StatusCode(500, "Error interno del servidor al crear la venta");
        }
    }

    [HttpGet("reporte")]
    public async Task<IActionResult> GenerarReporte(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? cliente)
    {
        try
        {
            var reporte = await _ventaService.GenerarReporteAsync(fechaInicio, fechaFin, cliente);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el reporte de ventas");
            return StatusCode(500, "Error interno del servidor al generar el reporte de ventas");
        }
    }
} 