using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MiBackend.DTOs.Requests;
using MiBackend.DTOs.Responses;
using MiBackend.Interfaces.Services;

namespace MiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CombosController : ControllerBase
{
    private readonly IComboService _comboService;

    public CombosController(IComboService comboService)
    {
        _comboService = comboService;
    }

    [HttpPost]
    public async Task<ActionResult<ComboResponse>> CreateCombo([FromBody] CreateComboRequest request)
    {
        try
        {
            var response = await _comboService.CreateComboAsync(request);
            return CreatedAtAction(nameof(GetComboById), new { id = response.ComboId }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear el combo", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ComboResponse>>> GetCombos()
    {
        try
        {
            var combos = await _comboService.GetCombosAsync();
            return Ok(combos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener los combos", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ComboResponse>> GetComboById(int id)
    {
        try
        {
            var combo = await _comboService.GetComboByIdAsync(id);
            return Ok(combo);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Combo con ID {id} no encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el combo", error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ComboResponse>>> GetActiveCombos()
    {
        try
        {
            var combos = await _comboService.GetActiveCombosAsync();
            return Ok(combos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener los combos activos", error = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateComboStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _comboService.UpdateComboStatusAsync(id, request.IsActive);
            return Ok(new { message = $"Estado del combo actualizado a {(request.IsActive ? "activo" : "inactivo")}" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Combo con ID {id} no encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar el estado del combo", error = ex.Message });
        }
    }

    [HttpGet("{id}/validate-stock")]
    public async Task<ActionResult<bool>> ValidateComboStock(int id, [FromQuery] decimal cantidad)
    {
        try
        {
            var isValid = await _comboService.ValidateComboStockAsync(id, cantidad);
            return Ok(new { hasStock = isValid });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Combo con ID {id} no encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al validar el stock del combo", error = ex.Message });
        }
    }

    [HttpGet("{id}/calculate-total")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> CalculateComboTotal(int id, [FromQuery, Required] decimal cantidad)
    {
        if (cantidad <= 0)
        {
            return BadRequest(new { message = "La cantidad debe ser mayor a 0" });
        }

        try
        {
            var total = await _comboService.CalculateComboTotalAsync(id, cantidad);
            return Ok(new { total });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Combo con ID {id} no encontrado" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al calcular el total del combo", error = ex.Message });
        }
    }
} 