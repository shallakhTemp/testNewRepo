using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Lookups;
using DocumentGenerator.Contracts.Resources.Lookups;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly IValidator<LookupModel> _lookupValidator;
    private readonly IValidator<LookupItemModel> _itemValidator;

    public LookupsController(
        ILookupService lookupService,
        IValidator<LookupModel> lookupValidator,
        IValidator<LookupItemModel> itemValidator)
    {
        _lookupService = lookupService;
        _lookupValidator = lookupValidator;
        _itemValidator = itemValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LookupResource>>>> GetLookups(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _lookupService.GetLookupsAsync(pageNumber, pageSize, searchTerm);
        return Ok(ApiResponse<PagedResult<LookupResource>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LookupResource>>> GetLookup(Guid id)
    {
        var result = await _lookupService.GetLookupByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<LookupResource>.ErrorResponse("Lookup not found"));

        return Ok(ApiResponse<LookupResource>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<LookupResource>>> CreateLookup([FromBody] LookupModel model)
    {
        try
        {
            var validation = await _lookupValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<LookupResource>.ErrorResponse(
                    "Validation failed",
                    validation.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var result = await _lookupService.CreateLookupAsync(model);
            return CreatedAtAction(nameof(GetLookup), new { id = result.Id },
                ApiResponse<LookupResource>.SuccessResponse(result, "Lookup created"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LookupResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<LookupResource>>> UpdateLookup(Guid id, [FromBody] LookupModel model)
    {
        try
        {
            var validation = await _lookupValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<LookupResource>.ErrorResponse(
                    "Validation failed",
                    validation.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var result = await _lookupService.UpdateLookupAsync(id, model);
            return Ok(ApiResponse<LookupResource>.SuccessResponse(result, "Lookup updated"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LookupResource>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LookupResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLookup(Guid id)
    {
        var result = await _lookupService.DeleteLookupAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Lookup not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Lookup deleted"));
    }

    [HttpPost("items")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<LookupItemResource>>> CreateLookupItem([FromBody] LookupItemModel model)
    {
        try
        {
            var validation = await _itemValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<LookupItemResource>.ErrorResponse(
                    "Validation failed",
                    validation.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var result = await _lookupService.CreateLookupItemAsync(model);
            return CreatedAtAction(nameof(GetLookup), new { id = model.LookupId },
                ApiResponse<LookupItemResource>.SuccessResponse(result, "Lookup item created"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LookupItemResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("items/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLookupItem(Guid id)
    {
        var result = await _lookupService.DeleteLookupItemAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Lookup item not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Lookup item deleted"));
    }
}