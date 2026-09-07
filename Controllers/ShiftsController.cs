using System.Security.Claims;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CLMS_APIs.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize]
[EnableCors("AllowReactApp")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly IValidator<ShiftCreateUpdateDto> _validator;
    private readonly ILogger<ShiftsController> _logger;

    public ShiftsController(
        IShiftService shiftService,
        IValidator<ShiftCreateUpdateDto> validator,
        ILogger<ShiftsController> logger)
    {
        _shiftService = shiftService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all shifts ordered by ShiftId.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ShiftListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var shifts = await _shiftService.GetAllShiftsAsync(cancellationToken);
        return Ok(shifts);
    }

    /// <summary>
    /// Retrieves a single shift by ShiftId.
    /// </summary>
    [HttpGet("{id:decimal}")]
    [ProducesResponseType(typeof(ShiftListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(decimal id, CancellationToken cancellationToken)
    {
        var shift = await _shiftService.GetShiftByIdAsync(id, cancellationToken);
        if (shift == null)
        {
            return NotFound(new { message = $"Shift with ID '{id}' was not found." });
        }

        return Ok(shift);
    }

    /// <summary>
    /// Creates a new shift. Validates timing, unique ShiftName, and calculates ShiftHours server-side.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShiftListDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ShiftCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                errors = validationResult.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
            });
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _shiftService.CreateShiftAsync(dto, userId, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.ShiftId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift: {ShiftName}", dto.ShiftName);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while creating the shift."
            });
        }
    }

    /// <summary>
    /// Updates an existing shift by ShiftId. Re-validates timing and recalculates ShiftHours.
    /// </summary>
    [HttpPut("{id:decimal}")]
    [ProducesResponseType(typeof(ShiftListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(decimal id, [FromBody] ShiftCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                errors = validationResult.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
            });
        }

        try
        {
            var userId = GetCurrentUserId();
            var updated = await _shiftService.UpdateShiftAsync(id, dto, userId, cancellationToken);

            if (updated == null)
            {
                return NotFound(new { message = $"Shift with ID '{id}' was not found." });
            }

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift ID: {ShiftId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while updating the shift."
            });
        }
    }

    /// <summary>
    /// Deletes a shift by ShiftId.
    /// </summary>
    [HttpDelete("{id:decimal}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(decimal id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _shiftService.DeleteShiftAsync(id, userId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = $"Shift with ID '{id}' was not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift ID: {ShiftId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while deleting the shift."
            });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("uid")?.Value
               ?? User.Identity?.Name;
    }
}
