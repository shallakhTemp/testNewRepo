using DocumentGenerator.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DocumentGenerator.DAL.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<UserClaimEntity> UserClaims => Set<UserClaimEntity>();
    public DbSet<RoleClaimEntity> RoleClaims => Set<RoleClaimEntity>();
    public DbSet<TemplateEntity> Templates => Set<TemplateEntity>();
    public DbSet<LookupEntity> Lookups => Set<LookupEntity>();
    public DbSet<LookupItemEntity> LookupItems => Set<LookupItemEntity>();
    public DbSet<TemplateAssetEntity> TemplateAssets => Set<TemplateAssetEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
