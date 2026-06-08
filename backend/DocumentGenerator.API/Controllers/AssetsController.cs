using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Assets;
using DocumentGenerator.Contracts.Resources.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TemplateAssetResource>>> Upload(
	    [FromForm] Guid templateId,
	    [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<TemplateAssetResource>.ErrorResponse("No file uploaded"));
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var model = new UploadAssetModel
            {
                TemplateId = templateId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileData = memoryStream.ToArray()
            };

            var result = await _assetService.UploadAssetAsync(model);
            return Ok(ApiResponse<TemplateAssetResource>.SuccessResponse(result, "File uploaded"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TemplateAssetResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("template/{templateId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TemplateAssetResource>>>> GetAssets(Guid templateId)
    {
        var result = await _assetService.GetAssetsByTemplateAsync(templateId);
        return Ok(ApiResponse<IEnumerable<TemplateAssetResource>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsset(Guid id)
    {
        try
        {
            var filePath = await _assetService.GetAssetPathAsync(id);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = "application/octet-stream";
            var fileName = Path.GetFileName(filePath);

            return File(fileBytes, contentType, fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsset(Guid id)
    {
        var result = await _assetService.DeleteAssetAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Asset not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Asset deleted"));
    }
}