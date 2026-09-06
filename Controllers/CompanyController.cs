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
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ICompanyService companyService, ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of all companies ordered by company name.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CompanyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var companies = await _companyService.GetAllCompaniesAsync(cancellationToken);
        return Ok(companies);
    }

    /// <summary>
    /// Retrieves full company details by ID for viewing/editing.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompanyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var company = await _companyService.GetCompanyByIdAsync(id, cancellationToken);
        if (company == null)
        {
            return NotFound(new { message = $"Company with ID '{id}' was not found." });
        }

        return Ok(company);
    }

    /// <summary>
    /// Creates a new company with auto-generated ID, duplicate name check, and optional logo upload (FormData).
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(CompanyDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromForm] CompanyCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessCreateAsync(dto, cancellationToken);
    }

    /// <summary>
    /// Creates a new company from JSON payload.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CreateFromJson([FromBody] CompanyCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessCreateAsync(dto, cancellationToken);
    }

    /// <summary>
    /// Updates an existing company and optionally replaces its logo (FormData).
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(CompanyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string id, [FromForm] CompanyCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessUpdateAsync(id, dto, cancellationToken);
    }

    /// <summary>
    /// Updates an existing company from JSON payload.
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UpdateFromJson(string id, [FromBody] CompanyCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        return await ProcessUpdateAsync(id, dto, cancellationToken);
    }

    /// <summary>
    /// Deletes a company and its associated logo files.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _companyService.DeleteCompanyAsync(id, userId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = $"Company with ID '{id}' was not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company ID: {CompanyId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while deleting the company."
            });
        }
    }

    private async Task<IActionResult> ProcessCreateAsync(CompanyCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _companyService.CreateCompanyAsync(dto, userId, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.CompanyId }, created);
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company: {CompanyName}", dto.CompanyName);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while creating the company."
            });
        }
    }

    private async Task<IActionResult> ProcessUpdateAsync(string id, CompanyCreateUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var updated = await _companyService.UpdateCompanyAsync(id, dto, userId, cancellationToken);

            if (updated == null)
            {
                return NotFound(new { message = $"Company with ID '{id}' was not found." });
            }

            return Ok(updated);
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company ID: {CompanyId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while updating the company."
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
