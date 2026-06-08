using DocumentGenerator.DAL.Entities;

namespace DocumentGenerator.DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<UserEntity> Users { get; }
    IGenericRepository<RoleEntity> Roles { get; }
    IGenericRepository<UserRoleEntity> UserRoles { get; }
    IGenericRepository<UserClaimEntity> UserClaims { get; }
    IGenericRepository<RoleClaimEntity> RoleClaims { get; }
    IGenericRepository<TemplateEntity> Templates { get; }
    IGenericRepository<LookupEntity> Lookups { get; }
    IGenericRepository<LookupItemEntity> LookupItems { get; }
    IGenericRepository<TemplateAssetEntity> TemplateAssets { get; }

    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
