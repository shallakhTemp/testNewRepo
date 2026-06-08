using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Lookups;
using DocumentGenerator.Contracts.Resources.Lookups;
using DocumentGenerator.DAL.Entities;
using DocumentGenerator.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentGenerator.Business.Services;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public LookupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<LookupResource>> GetLookupsAsync(int pageNumber, int pageSize, string? searchTerm = null)
    {
        System.Linq.Expressions.Expression<Func<LookupEntity, bool>>? filter = string.IsNullOrWhiteSpace(searchTerm)
            ? null
            : l => l.Name.Contains(searchTerm);

        var result = await _unitOfWork.Lookups.GetPagedAsync(
            pageNumber, pageSize, filter,
            q => q.OrderByDescending(l => l.CreatedOn));

        var lookups = new List<LookupResource>();
        foreach (var entity in result.Items)
        {
            var lookup = await _unitOfWork.Lookups.GetQueryable()
                .Include(l => l.Items)
                .Where(l => l.Id == entity.Id)
                .Select(l => new LookupResource
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Items = l.Items.OrderBy(i => i.DisplayOrder).Select(i => new LookupItemResource
                    {
                        Id = i.Id,
                        LookupId = i.LookupId,
                        Value = i.Value,
                        Text = i.Text,
                        DisplayOrder = i.DisplayOrder
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (lookup != null)
                lookups.Add(lookup);
        }

        return new PagedResult<LookupResource>
        {
            Items = lookups,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<LookupResource?> GetLookupByIdAsync(Guid id)
    {
        var lookup = await _unitOfWork.Lookups.GetQueryable()
            .Include(l => l.Items)
            .Where(l => l.Id == id)
            .Select(l => new LookupResource
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Items = l.Items.OrderBy(i => i.DisplayOrder).Select(i => new LookupItemResource
                {
                    Id = i.Id,
                    LookupId = i.LookupId,
                    Value = i.Value,
                    Text = i.Text,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return lookup;
    }

    public async Task<LookupResource> CreateLookupAsync(LookupModel model)
    {
        if (await _unitOfWork.Lookups.ExistsAsync(l => l.Name == model.Name))
            throw new InvalidOperationException($"Lookup with name '{model.Name}' already exists");

        var entity = new LookupEntity
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Lookups.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return new LookupResource
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Items = new List<LookupItemResource>()
        };
    }

    public async Task<LookupResource> UpdateLookupAsync(Guid id, LookupModel model)
    {
        var entity = await _unitOfWork.Lookups.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Lookup with id {id} not found");

        if (await _unitOfWork.Lookups.ExistsAsync(l => l.Name == model.Name && l.Id != id))
            throw new InvalidOperationException($"Lookup with name '{model.Name}' already exists");

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.ModifiedOn = DateTime.UtcNow;

        await _unitOfWork.Lookups.UpdateAsync(entity);
        await _unitOfWork.CompleteAsync();

        var items = await _unitOfWork.LookupItems.FindAsync(i => i.LookupId == id);

        return new LookupResource
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Items = items.OrderBy(i => i.DisplayOrder).Select(i => new LookupItemResource
            {
                Id = i.Id,
                LookupId = i.LookupId,
                Value = i.Value,
                Text = i.Text,
                DisplayOrder = i.DisplayOrder
            }).ToList()
        };
    }

    public async Task<bool> DeleteLookupAsync(Guid id)
    {
        var entity = await _unitOfWork.Lookups.GetByIdAsync(id);
        if (entity == null) return false;

        var items = await _unitOfWork.LookupItems.FindAsync(i => i.LookupId == id);
        foreach (var item in items)
        {
            await _unitOfWork.LookupItems.DeleteAsync(item);
        }

        await _unitOfWork.Lookups.DeleteAsync(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<LookupItemResource> CreateLookupItemAsync(LookupItemModel model)
    {
        var lookup = await _unitOfWork.Lookups.GetByIdAsync(model.LookupId)
            ?? throw new KeyNotFoundException($"Lookup with id {model.LookupId} not found");

        var entity = new LookupItemEntity
        {
            Id = Guid.NewGuid(),
            LookupId = model.LookupId,
            Value = model.Value,
            Text = model.Text,
            DisplayOrder = model.DisplayOrder
        };

        await _unitOfWork.LookupItems.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return new LookupItemResource
        {
            Id = entity.Id,
            LookupId = entity.LookupId,
            Value = entity.Value,
            Text = entity.Text,
            DisplayOrder = entity.DisplayOrder
        };
    }

    public async Task<bool> DeleteLookupItemAsync(Guid id)
    {
        var entity = await _unitOfWork.LookupItems.GetByIdAsync(id);
        if (entity == null) return false;

        await _unitOfWork.LookupItems.DeleteAsync(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}