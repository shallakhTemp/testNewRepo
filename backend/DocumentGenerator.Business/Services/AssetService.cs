using Microsoft.Extensions.Configuration;
using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Models.Assets;
using DocumentGenerator.Contracts.Resources.Assets;
using DocumentGenerator.DAL.Entities;
using DocumentGenerator.DAL.Interfaces;

namespace DocumentGenerator.Business.Services;

public class AssetService : IAssetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _assetPath;

    public AssetService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _assetPath = configuration["AssetSettings:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        Directory.CreateDirectory(_assetPath);
    }

    public async Task<TemplateAssetResource> UploadAssetAsync(UploadAssetModel model)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(model.TemplateId)
            ?? throw new KeyNotFoundException($"Template with id {model.TemplateId} not found");

        var templatePath = Path.Combine(_assetPath, model.TemplateId.ToString());
        Directory.CreateDirectory(templatePath);

        var uniqueFileName = $"{Guid.NewGuid()}_{model.FileName}";
        var filePath = Path.Combine(templatePath, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, model.FileData);

        var entity = new TemplateAssetEntity
        {
            Id = Guid.NewGuid(),
            TemplateId = model.TemplateId,
            FileName = model.FileName,
            FilePath = filePath,
            ContentType = model.ContentType,
            UploadedOn = DateTime.UtcNow
        };

        await _unitOfWork.TemplateAssets.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return new TemplateAssetResource
        {
            Id = entity.Id,
            TemplateId = entity.TemplateId,
            FileName = entity.FileName,
            FilePath = entity.FilePath,
            ContentType = entity.ContentType,
            Url = $"/api/assets/{entity.Id}"
        };
    }

    public async Task<IEnumerable<TemplateAssetResource>> GetAssetsByTemplateAsync(Guid templateId)
    {
        var assets = await _unitOfWork.TemplateAssets.FindAsync(a => a.TemplateId == templateId);

        return assets.Select(a => new TemplateAssetResource
        {
            Id = a.Id,
            TemplateId = a.TemplateId,
            FileName = a.FileName,
            FilePath = a.FilePath,
            ContentType = a.ContentType,
            Url = $"/api/assets/{a.Id}"
        });
    }

    public async Task<bool> DeleteAssetAsync(Guid id)
    {
        var entity = await _unitOfWork.TemplateAssets.GetByIdAsync(id);
        if (entity == null) return false;

        if (File.Exists(entity.FilePath))
        {
            File.Delete(entity.FilePath);
        }

        await _unitOfWork.TemplateAssets.DeleteAsync(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<string> GetAssetPathAsync(Guid assetId)
    {
        var entity = await _unitOfWork.TemplateAssets.GetByIdAsync(assetId)
            ?? throw new KeyNotFoundException($"Asset with id {assetId} not found");

        return entity.FilePath;
    }
}