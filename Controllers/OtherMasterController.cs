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
[Route("api/other-master")]
[Authorize]
[EnableCors("AllowReactApp")]
public class OtherMasterController : ControllerBase
{
    private readonly IOtherMasterService _otherMasterService;
    private readonly IValidator<OtherMasterCreateDto> _createValidator;
    private readonly ILogger<OtherMasterController> _logger;

    public OtherMasterController(
        IOtherMasterService otherMasterService,
        IValidator<OtherMasterCreateDto> createValidator,
        ILogger<OtherMasterController> logger)
    {
        _otherMasterService = otherMasterService;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a distinct list of categories (MasterId, MasterName) for dropdowns.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<MasterCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _otherMasterService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves all OtherMaster entries, optionally filtered by category (masterId).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OtherMasterListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? masterId, CancellationToken cancellationToken)
    {
        var list = await _otherMasterService.GetAllAsync(masterId, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves active lookup options (MasterTypeId, MasterType) by category name (e.g. 'Contractor Type').
    /// </summary>
    [HttpGet("lookup/{masterName}")]
    [ProducesResponseType(typeof(List<OtherMasterLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLookup(string masterName, CancellationToken cancellationToken)
    {
        var lookups = await _otherMasterService.GetLookupByMasterNameAsync(masterName, cancellationToken);
        return Ok(lookups);
    }

    /// <summary>
    /// Retrieves a single OtherMaster row by MasterTypeId.
    /// </summary>
    [HttpGet("{masterTypeId:int}")]
    [ProducesResponseType(typeof(OtherMasterListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int masterTypeId, CancellationToken cancellationToken)
    {
        var item = await _otherMasterService.GetByIdAsync(masterTypeId, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"OtherMaster entry with MasterTypeId '{masterTypeId}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a new MasterType entry under an existing category (MasterId) or new category (NewMasterName).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OtherMasterListDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] OtherMasterCreateDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
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
            var created = await _otherMasterService.CreateAsync(dto, userId, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { masterTypeId = created.MasterTypeId }, created);
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
            _logger.LogError(ex, "Error creating OtherMaster entry for {MasterType}", dto.MasterType);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while creating the OtherMaster entry."
            });
        }
    }

    /// <summary>
    /// Updates MasterType and Description of an existing OtherMaster entry. Category is immutable.
    /// </summary>
    [HttpPut("{masterTypeId:int}")]
    [ProducesResponseType(typeof(OtherMasterListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int masterTypeId, [FromBody] OtherMasterUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var updated = await _otherMasterService.UpdateAsync(masterTypeId, dto, userId, cancellationToken);

            if (updated == null)
            {
                return NotFound(new { message = $"OtherMaster entry with MasterTypeId '{masterTypeId}' was not found." });
            }

            return Ok(updated);
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating OtherMaster entry ID: {MasterTypeId}", masterTypeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while updating the OtherMaster entry."
            });
        }
    }

    /// <summary>
    /// Deletes an OtherMaster entry by MasterTypeId.
    /// </summary>
    [HttpDelete("{masterTypeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int masterTypeId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _otherMasterService.DeleteAsync(masterTypeId, userId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = $"OtherMaster entry with MasterTypeId '{masterTypeId}' was not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting OtherMaster entry ID: {MasterTypeId}", masterTypeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while deleting the OtherMaster entry."
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
