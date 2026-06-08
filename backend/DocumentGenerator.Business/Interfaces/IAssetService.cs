using DocumentGenerator.Contracts.Models.Assets;
using DocumentGenerator.Contracts.Resources.Assets;

namespace DocumentGenerator.Business.Interfaces;

public interface IAssetService
{
    Task<TemplateAssetResource> UploadAssetAsync(UploadAssetModel model);
    Task<IEnumerable<TemplateAssetResource>> GetAssetsByTemplateAsync(Guid templateId);
    Task<bool> DeleteAssetAsync(Guid id);
    Task<string> GetAssetPathAsync(Guid assetId);
}