using System.Security.Claims;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CLMS_APIs.Controllers;

[ApiController]
[Route("api/holidays")]
[Authorize]
[EnableCors("AllowReactApp")]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayService _holidayService;
    private readonly ILogger<HolidaysController> _logger;

    public HolidaysController(IHolidayService holidayService, ILogger<HolidaysController> logger)
    {
        _holidayService = holidayService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all holidays ordered by date ascending, with optional year filter.
    /// </summary>
    /// <param name="year">Optional 4-digit year filter (e.g. 2026).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<HolidayDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? year, CancellationToken cancellationToken)
    {
        var holidays = await _holidayService.GetAllHolidaysAsync(year, cancellationToken);
        return Ok(holidays);
    }

    /// <summary>
    /// Retrieves a single holiday by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var holiday = await _holidayService.GetHolidayByIdAsync(id, cancellationToken);
        if (holiday == null)
        {
            return NotFound(new { message = $"Holiday with ID '{id}' was not found." });
        }

        return Ok(holiday);
    }

    /// <summary>
    /// Creates a new holiday record. Rejects past dates and duplicates.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] HolidayCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _holidayService.CreateHolidayAsync(dto, userId, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
            _logger.LogError(ex, "Error creating holiday: {HolidayDesc}", dto.HolidayDesc);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while creating the holiday."
            });
        }
    }

    /// <summary>
    /// Updates an existing holiday by ID.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] HolidayCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var updated = await _holidayService.UpdateHolidayAsync(id, dto, userId, cancellationToken);

            if (updated == null)
            {
                return NotFound(new { message = $"Holiday with ID '{id}' was not found." });
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
            _logger.LogError(ex, "Error updating holiday ID: {HolidayId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while updating the holiday."
            });
        }
    }

    /// <summary>
    /// Deletes a holiday by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _holidayService.DeleteHolidayAsync(id, userId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = $"Holiday with ID '{id}' was not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting holiday ID: {HolidayId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while deleting the holiday."
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
