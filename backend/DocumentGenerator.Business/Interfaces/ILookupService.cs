using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Lookups;
using DocumentGenerator.Contracts.Resources.Lookups;

namespace DocumentGenerator.Business.Interfaces;

public interface ILookupService
{
    Task<PagedResult<LookupResource>> GetLookupsAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<LookupResource?> GetLookupByIdAsync(Guid id);
    Task<LookupResource> CreateLookupAsync(LookupModel model);
    Task<LookupResource> UpdateLookupAsync(Guid id, LookupModel model);
    Task<bool> DeleteLookupAsync(Guid id);
    Task<LookupItemResource> CreateLookupItemAsync(LookupItemModel model);
    Task<bool> DeleteLookupItemAsync(Guid id);
}