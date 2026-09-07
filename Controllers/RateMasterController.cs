using System.Security.Claims;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs.RateMaster;
using CLMS_APIs.Services.RateMaster;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CLMS_APIs.Controllers;

[ApiController]
[Route("api/rate-master")]
[Authorize]
[EnableCors("AllowReactApp")]
public class RateMasterController : ControllerBase
{
    private readonly IRateMasterService _rateMasterService;
    private readonly IValidator<CreateRateMasterRequest> _createValidator;
    private readonly IValidator<UpdateRateMasterRequest> _updateValidator;
    private readonly ILogger<RateMasterController> _logger;

    public RateMasterController(
        IRateMasterService rateMasterService,
        IValidator<CreateRateMasterRequest> createValidator,
        IValidator<UpdateRateMasterRequest> updateValidator,
        ILogger<RateMasterController> logger)
    {
        _rateMasterService = rateMasterService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of rate master entries with optional filtering by category type (Labour/Employee), date range, and category.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RateMasterListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] RateMasterQueryRequest query, CancellationToken cancellationToken)
    {
        var result = await _rateMasterService.GetRatesAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves category lookup list from OtherMaster (masterName = 'Labour Type Master').
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<RateMasterCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _rateMasterService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a single Rate Master record by RID.
    /// </summary>
    [HttpGet("{rid:decimal}")]
    [ProducesResponseType(typeof(RateMasterDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(decimal rid, CancellationToken cancellationToken)
    {
        var rate = await _rateMasterService.GetRateByIdAsync(rid, cancellationToken);
        if (rate == null)
        {
            return NotFound(new { message = $"Rate master record with RID '{rid}' was not found." });
        }

        return Ok(rate);
    }

    /// <summary>
    /// Creates a new Rate Master entry with server-side Gross and RatePerDay calculations, date overlap checks, and concurrency-safe RID generation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RateMasterDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRateMasterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validationResult.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
            });
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _rateMasterService.CreateRateAsync(request, userId, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { rid = created.Rid }, created);
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
            _logger.LogError(ex, "Error creating Rate Master entry for category {LabourCatId}", request.LabourCatId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while creating the Rate Master entry."
            });
        }
    }

    /// <summary>
    /// Updates an existing Rate Master record by RID.
    /// </summary>
    [HttpPut("{rid:decimal}")]
    [ProducesResponseType(typeof(RateMasterDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(decimal rid, [FromBody] UpdateRateMasterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validationResult.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
            });
        }

        try
        {
            var userId = GetCurrentUserId();
            var updated = await _rateMasterService.UpdateRateAsync(rid, request, userId, cancellationToken);

            if (updated == null)
            {
                return NotFound(new { message = $"Rate master record with RID '{rid}' was not found." });
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
            _logger.LogError(ex, "Error updating Rate Master RID: {Rid}", rid);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while updating the Rate Master entry."
            });
        }
    }

    /// <summary>
    /// Deletes a Rate Master record by RID with optional category safety check.
    /// </summary>
    [HttpDelete("{rid:decimal}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(decimal rid, [FromQuery] string? categoryType, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _rateMasterService.DeleteRateAsync(rid, categoryType, userId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = $"Rate master record with RID '{rid}' was not found." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Rate Master RID: {Rid}", rid);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while deleting the Rate Master entry."
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
