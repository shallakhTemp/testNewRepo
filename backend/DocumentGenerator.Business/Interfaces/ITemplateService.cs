using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Templates;
using DocumentGenerator.Contracts.Resources.Templates;

namespace DocumentGenerator.Business.Interfaces;

public interface ITemplateService
{
    Task<PagedResult<TemplateResource>> GetTemplatesAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<TemplateResource?> GetTemplateByIdAsync(Guid id);
    Task<TemplateResource> CreateTemplateAsync(TemplateSaveModel model);
    Task<TemplateResource> UpdateTemplateAsync(Guid id, TemplateSaveModel model);
    Task<TemplateResource> UpdateDesignAsync(Guid id, string designJson);
    Task<bool> DeleteTemplateAsync(Guid id);
    Task<bool> ToggleTemplateActiveAsync(Guid id);
    Task<string> RenderTemplateHtmlAsync(Guid id, Dictionary<string, string> fieldValues);
}