using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.Interfaces.Services;

namespace MiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CombosController : ControllerBase
{
    private readonly IComboService _comboService;
    private readonly ILogger<CombosController> _logger;

    public CombosController(IComboService comboService, ILogger<CombosController> logger)
    {
        _comboService = comboService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var combos = await _comboService.GetAllAsync();
            return Ok(combos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener combos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var combo = await _comboService.GetByIdAsync(id);
            if (combo == null)
            {
                return NotFound(new { message = $"Combo con ID {id} no encontrado" });
            }
            return Ok(combo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener combo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComboRequest request)
    {
        try
        {
            var combo = await _comboService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = combo.ComboId }, combo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear combo");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateComboRequest request)
    {
        try
        {
            var combo = await _comboService.UpdateAsync(id, request);
            return Ok(combo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar combo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _comboService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar combo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPatch("{id}/toggle-estado")]
    public async Task<IActionResult> ToggleEstado(int id)
    {
        try
        {
            var combo = await _comboService.ToggleEstadoAsync(id);
            return Ok(combo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del combo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
} 