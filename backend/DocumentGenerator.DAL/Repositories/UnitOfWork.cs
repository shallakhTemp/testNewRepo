using DocumentGenerator.DAL.Data;
using DocumentGenerator.DAL.Entities;
using DocumentGenerator.DAL.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DocumentGenerator.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private IGenericRepository<UserEntity>? _users;
    private IGenericRepository<RoleEntity>? _roles;
    private IGenericRepository<UserRoleEntity>? _userRoles;
    private IGenericRepository<UserClaimEntity>? _userClaims;
    private IGenericRepository<RoleClaimEntity>? _roleClaims;
    private IGenericRepository<TemplateEntity>? _templates;
    private IGenericRepository<LookupEntity>? _lookups;
    private IGenericRepository<LookupItemEntity>? _lookupItems;
    private IGenericRepository<TemplateAssetEntity>? _templateAssets;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<UserEntity> Users => _users ??= new GenericRepository<UserEntity>(_context);
    public IGenericRepository<RoleEntity> Roles => _roles ??= new GenericRepository<RoleEntity>(_context);
    public IGenericRepository<UserRoleEntity> UserRoles => _userRoles ??= new GenericRepository<UserRoleEntity>(_context);
    public IGenericRepository<UserClaimEntity> UserClaims => _userClaims ??= new GenericRepository<UserClaimEntity>(_context);
    public IGenericRepository<RoleClaimEntity> RoleClaims => _roleClaims ??= new GenericRepository<RoleClaimEntity>(_context);
    public IGenericRepository<TemplateEntity> Templates => _templates ??= new GenericRepository<TemplateEntity>(_context);
    public IGenericRepository<LookupEntity> Lookups => _lookups ??= new GenericRepository<LookupEntity>(_context);
    public IGenericRepository<LookupItemEntity> LookupItems => _lookupItems ??= new GenericRepository<LookupItemEntity>(_context);
    public IGenericRepository<TemplateAssetEntity> TemplateAssets => _templateAssets ??= new GenericRepository<TemplateAssetEntity>(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}