using System.Text.Json;
using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Templates;
using DocumentGenerator.Contracts.Resources.Templates;
using DocumentGenerator.DAL.Entities;
using DocumentGenerator.DAL.Interfaces;

namespace DocumentGenerator.Business.Services;

public class TemplateService : ITemplateService
{
    private readonly IUnitOfWork _unitOfWork;

    public TemplateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<TemplateResource>> GetTemplatesAsync(int pageNumber, int pageSize, string? searchTerm = null)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? null
            : (System.Linq.Expressions.Expression<Func<TemplateEntity, bool>>)(t =>
                t.Name.Contains(searchTerm) || t.Code.Contains(searchTerm));

        var result = await _unitOfWork.Templates.GetPagedAsync(
            pageNumber, pageSize, filter,
            q => q.OrderByDescending(t => t.CreatedOn));

        return new PagedResult<TemplateResource>
        {
            Items = result.Items.Select(MapToResource).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<TemplateResource?> GetTemplateByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Templates.GetByIdAsync(id);
        return entity == null ? null : MapToResource(entity);
    }

    public async Task<TemplateResource> CreateTemplateAsync(TemplateSaveModel model)
    {
        if (await _unitOfWork.Templates.ExistsAsync(t => t.Code == model.Code))
            throw new InvalidOperationException($"Template with code '{model.Code}' already exists");

        var entity = new TemplateEntity
        {
            Id = Guid.NewGuid(),
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            DesignJson = model.DesignJson,
            IsActive = model.IsActive,
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Templates.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return MapToResource(entity);
    }

    public async Task<TemplateResource> UpdateTemplateAsync(Guid id, TemplateSaveModel model)
    {
        var entity = await _unitOfWork.Templates.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Template with id {id} not found");

        if (await _unitOfWork.Templates.ExistsAsync(t => t.Code == model.Code && t.Id != id))
            throw new InvalidOperationException($"Template with code '{model.Code}' already exists");

        entity.Code = model.Code;
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.DesignJson = model.DesignJson;
        entity.IsActive = model.IsActive;
        entity.ModifiedOn = DateTime.UtcNow;

        await _unitOfWork.Templates.UpdateAsync(entity);
        await _unitOfWork.CompleteAsync();

        return MapToResource(entity);
    }

    public async Task<TemplateResource> UpdateDesignAsync(Guid id, string designJson)
    {
        var entity = await _unitOfWork.Templates.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Template with id {id} not found");

        entity.DesignJson = designJson;
        entity.ModifiedOn = DateTime.UtcNow;

        await _unitOfWork.Templates.UpdateAsync(entity);
        await _unitOfWork.CompleteAsync();

        return MapToResource(entity);
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        var entity = await _unitOfWork.Templates.GetByIdAsync(id);
        if (entity == null) return false;

        await _unitOfWork.Templates.DeleteAsync(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> ToggleTemplateActiveAsync(Guid id)
    {
        var entity = await _unitOfWork.Templates.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Template with id {id} not found");

        entity.IsActive = !entity.IsActive;
        entity.ModifiedOn = DateTime.UtcNow;

        await _unitOfWork.Templates.UpdateAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.IsActive;
    }

    public async Task<string> RenderTemplateHtmlAsync(Guid id, Dictionary<string, string> fieldValues)
    {
        var entity = await _unitOfWork.Templates.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Template with id {id} not found");

        DesignerDocument? doc;
        try
        {
            doc = JsonSerializer.Deserialize<DesignerDocument>(entity.DesignJson);
        }
        catch
        {
            throw new InvalidOperationException("Invalid design JSON");
        }

        if (doc == null)
            throw new InvalidOperationException("Template has no design");

        var html = BuildRenderHtml(doc, fieldValues);
        return html;
    }

    private static string BuildRenderHtml(DesignerDocument doc, Dictionary<string, string> fieldValues)
    {
        var page = doc.PageSettings;
        var elementsHtml = new List<string>();

        foreach (var el in doc.Elements)
        {
            var style = el.Styles;
            var posStyle = $"position: absolute; left: {el.X}px; top: {el.Y}px; width: {el.Width}px; height: {el.Height}px;";
            var textStyle = $"font-size: {style.FontSize}px; color: {style.Color}; text-align: {style.TextAlign}; padding: {style.Padding}px;";
            if (style.Bold) textStyle += " font-weight: bold;";
            if (style.Italic) textStyle += " font-style: italic;";
            if (!string.IsNullOrEmpty(style.FontFamily)) textStyle += $" font-family: {style.FontFamily};";
            if (!string.IsNullOrEmpty(style.BackgroundColor)) textStyle += $" background-color: {style.BackgroundColor};";
            if (!string.IsNullOrEmpty(style.BorderTop)) textStyle += $" border-top: {style.BorderTop};";
            if (!string.IsNullOrEmpty(style.BorderBottom)) textStyle += $" border-bottom: {style.BorderBottom};";
            if (!string.IsNullOrEmpty(style.BorderLeft)) textStyle += $" border-left: {style.BorderLeft};";
            if (!string.IsNullOrEmpty(style.BorderRight)) textStyle += $" border-right: {style.BorderRight};";

            string innerHtml;

            switch (el.Type)
            {
                case DesignerElementType.Text:
                case DesignerElementType.Header:
                case DesignerElementType.Footer:
                    innerHtml = el.Content ?? "";
                    break;

                case DesignerElementType.Field:
                    var value = fieldValues?.ContainsKey(el.Id) == true ? fieldValues[el.Id] : "";
                    innerHtml = $"<span style=\"border-bottom: 1px solid #999; min-width: 100px; display: inline-block;\">{value}</span>";
                    if (!string.IsNullOrEmpty(el.Label))
                        innerHtml = $"<div style=\"font-size: {style.FontSize - 2}px; color: #666;\">{el.Label}</div>{innerHtml}";
                    break;

                case DesignerElementType.Signature:
                    innerHtml = "<div style=\"border-top: 1px solid #000; width: 150px; margin-top: 20px;\"></div>" +
                                (!string.IsNullOrEmpty(el.Label) ? $"<div style=\"font-size: 12px; text-align: center;\">{el.Label}</div>" : "");
                    break;

                case DesignerElementType.Divider:
                    innerHtml = "<hr style=\"border: none; border-top: 1px solid #000; margin: 0;\">";
                    break;

                case DesignerElementType.Image:
                    innerHtml = !string.IsNullOrEmpty(el.ImageSrc)
                        ? $"<img src=\"{el.ImageSrc}\" style=\"width: 100%; height: 100%; object-fit: contain;\" />"
                        : "<div style=\"background: #eee; text-align: center; padding: 10px;\">[Image]</div>";
                    break;

                case DesignerElementType.Table:
                    var cols = el.TableColumns ?? new List<string>();
                    var headerCells = string.Join("", cols.Select(c => $"<th style=\"border: 1px solid #000; padding: 4px;\">{c}</th>"));
                    innerHtml = $"<table style=\"width: 100%; border-collapse: collapse;\"><thead><tr>{headerCells}</tr></thead><tbody></tbody></table>";
                    break;

                default:
                    innerHtml = "";
                    break;
            }

            elementsHtml.Add($"<div style=\"{posStyle} {textStyle}\">{innerHtml}</div>");
        }

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        @page {{
            size: {page.Width} {page.Height};
            margin: {page.MarginTop} {page.MarginRight} {page.MarginBottom} {page.MarginLeft};
        }}
        body {{
            position: relative;
            width: 100%;
            height: 100%;
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
        }}
    </style>
</head>
<body>
    {string.Join("\n    ", elementsHtml)}
</body>
</html>";
    }

    private static TemplateResource MapToResource(TemplateEntity entity)
    {
        return new TemplateResource
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            DesignJson = entity.DesignJson,
            IsActive = entity.IsActive,
            CreatedOn = entity.CreatedOn,
            ModifiedOn = entity.ModifiedOn
        };
    }
}