using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Templates;
using DocumentGenerator.Contracts.Resources.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TemplateResource>>>> GetTemplates(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _templateService.GetTemplatesAsync(pageNumber, pageSize, searchTerm);
        return Ok(ApiResponse<PagedResult<TemplateResource>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TemplateResource>>> GetTemplate(Guid id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<TemplateResource>.ErrorResponse("Template not found"));

        return Ok(ApiResponse<TemplateResource>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TemplateResource>>> CreateTemplate([FromBody] TemplateSaveModel model)
    {
        try
        {
            var result = await _templateService.CreateTemplateAsync(model);
            return CreatedAtAction(nameof(GetTemplate), new { id = result.Id },
                ApiResponse<TemplateResource>.SuccessResponse(result, "Template created"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<TemplateResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TemplateResource>>> UpdateTemplate(Guid id, [FromBody] TemplateSaveModel model)
    {
        try
        {
            var result = await _templateService.UpdateTemplateAsync(id, model);
            return Ok(ApiResponse<TemplateResource>.SuccessResponse(result, "Template updated"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TemplateResource>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<TemplateResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/design")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TemplateResource>>> UpdateDesign(Guid id, [FromBody] string designJson)
    {
        try
        {
            var result = await _templateService.UpdateDesignAsync(id, designJson);
            return Ok(ApiResponse<TemplateResource>.SuccessResponse(result, "Design updated"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TemplateResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(Guid id)
    {
        var result = await _templateService.DeleteTemplateAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Template not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Template deleted"));
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(Guid id)
    {
        try
        {
            var isActive = await _templateService.ToggleTemplateActiveAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(isActive, isActive ? "Template activated" : "Template deactivated"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id}/render")]
    public async Task<IActionResult> RenderTemplate(Guid id, [FromBody] TemplateFillModel model)
    {
        try
        {
            var html = await _templateService.RenderTemplateHtmlAsync(id, model.FieldValues);
            return Content(html, "text/html");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}