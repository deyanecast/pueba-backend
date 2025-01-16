using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;
using MiBackend.Helpers;

namespace MiBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVentasByDate([FromQuery] string? date)
        {
            try
            {
                var fechaConsulta = date != null 
                    ? DateTimeHelper.ParseFlexible(date).ToStartOfDay()
                    : DateTime.UtcNow.ToStartOfDay();

                var ventas = await _ventaService.GetVentasByDateAsync(fechaConsulta);
                return Ok(ventas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener ventas: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVentaById(int id)
        {
            try
            {
                var venta = await _ventaService.GetVentaByIdAsync(id);
                if (venta == null)
                    return NotFound(new { message = $"Venta con ID {id} no encontrada" });

                return Ok(venta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener venta: {ex.Message}" });
            }
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetVentasByDateRange([FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                var fechaInicio = DateTimeHelper.ParseFlexible(startDate);
                var fechaFin = DateTimeHelper.ParseFlexible(endDate);

                if (fechaInicio > fechaFin)
                    return BadRequest(new { message = "La fecha inicial debe ser menor o igual a la fecha final" });

                var ventas = await _ventaService.GetVentasByDateRangeAsync(fechaInicio, fechaFin);
                return Ok(new { 
                    startDate = fechaInicio.ToString("yyyy-MM-dd"),
                    endDate = fechaFin.ToString("yyyy-MM-dd"),
                    ventas = ventas ?? new List<VentaResponse>()
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener ventas por rango de fechas: {ex.Message}" });
            }
        }

        [HttpGet("total/date")]
        public async Task<IActionResult> GetTotalVentasByDate([FromQuery] string date)
        {
            try
            {
                var fechaConsulta = DateTimeHelper.ParseFlexible(date).ToStartOfDay();
                var total = await _ventaService.GetTotalVentasByDateAsync(fechaConsulta);
                return Ok(new { total });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener total de ventas: {ex.Message}" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var dashboardData = await _ventaService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener datos del dashboard: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVenta([FromBody] CreateVentaRequest request)
        {
            try
            {
                var venta = await _ventaService.CreateVentaAsync(request);
                return CreatedAtAction(nameof(GetVentaById), new { id = venta.VentaId }, venta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear venta: {ex.Message}" });
            }
        }
    }
} 