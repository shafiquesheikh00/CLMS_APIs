using System.Security.Claims;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CLMS_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableCors("AllowReactApp")]
public class ContractorsController : ControllerBase
{
    private readonly IContractorService _contractorService;
    private readonly ILogger<ContractorsController> _logger;

    public ContractorsController(IContractorService contractorService, ILogger<ContractorsController> logger)
    {
        _contractorService = contractorService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of contractors, optionally filtered by active validity date (ValidDt >= today).
    /// </summary>
    /// <param name="active">Optional boolean: true for active, false for expired/inactive, null for all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<ContractorListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? active, CancellationToken cancellationToken)
    {
        var contractors = await _contractorService.GetContractorsAsync(active, cancellationToken);
        return Ok(contractors);
    }

    /// <summary>
    /// Retrieves contractor types from OtherMaster for dropdowns.
    /// </summary>
    [HttpGet("lookup/types")]
    [ProducesResponseType(typeof(List<ContractorTypeLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
    {
        var types = await _contractorService.GetContractorTypesAsync(cancellationToken);
        return Ok(types);
    }

    /// <summary>
    /// Retrieves full details of a specific contractor by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContractorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(decimal id, CancellationToken cancellationToken)
    {
        var contractor = await _contractorService.GetContractorByIdAsync(id, cancellationToken);
        if (contractor == null)
        {
            return NotFound(new { message = $"Contractor with ID '{id}' was not found." });
        }

        return Ok(contractor);
    }

    /// <summary>
    /// Creates a new contractor (FormData with documents).
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(ContractorDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromForm] ContractorCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessCreateAsync(dto, cancellationToken);
    }

    /// <summary>
    /// Creates a new contractor from JSON payload.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CreateFromJson([FromBody] ContractorCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessCreateAsync(dto, cancellationToken);
    }

    /// <summary>
    /// Updates an existing contractor (FormData with documents).
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(ContractorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(decimal id, [FromForm] ContractorCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessUpdateAsync(id, dto, cancellationToken);
    }

    /// <summary>
    /// Updates an existing contractor from JSON payload.
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UpdateFromJson(decimal id, [FromBody] ContractorCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessUpdateAsync(id, dto, cancellationToken);
    }

    /// <summary>
    /// Deletes a contractor if no active employees are linked.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(decimal id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _contractorService.DeleteContractorAsync(id, userId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = $"Contractor with ID '{id}' was not found." });
            }

            return NoContent();
        }
        catch (ContractorConflictException ex)
        {
            return Conflict(new { conflictField = ex.ConflictField, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contractor ID: {ContractorId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while deleting the contractor."
            });
        }
    }

    private async Task<IActionResult> ProcessCreateAsync(ContractorCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _contractorService.CreateContractorAsync(dto, userId, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.ContractorId }, created);
        }
        catch (ContractorConflictException ex)
        {
            return Conflict(new { conflictField = ex.ConflictField, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contractor: {ContractorName}", dto.Name);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while creating the contractor."
            });
        }
    }

    private async Task<IActionResult> ProcessUpdateAsync(decimal id, ContractorCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var updated = await _contractorService.UpdateContractorAsync(id, dto, userId, cancellationToken);

            if (updated == null)
            {
                return NotFound(new { message = $"Contractor with ID '{id}' was not found." });
            }

            return Ok(updated);
        }
        catch (ContractorConflictException ex)
        {
            return Conflict(new { conflictField = ex.ConflictField, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contractor ID: {ContractorId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while updating the contractor."
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
