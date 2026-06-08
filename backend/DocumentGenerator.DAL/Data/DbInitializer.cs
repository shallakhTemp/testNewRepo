using DocumentGenerator.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentGenerator.DAL.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Description = "Administrator role with full access",
                CreatedOn = DateTime.UtcNow
            };
            await context.Roles.AddAsync(adminRole);
        }

        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            userRole = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = "User",
                Description = "Standard user role",
                CreatedOn = DateTime.UtcNow
            };
            await context.Roles.AddAsync(userRole);
        }

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@admin.com");
        if (adminUser == null)
        {
            adminUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "admin@admin.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            var userRoleAssign = new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                RoleId = adminRole!.Id,
                AssignedOn = DateTime.UtcNow
            };
            await context.UserRoles.AddAsync(userRoleAssign);

            var adminClaims = new List<UserClaimEntity>
            {
                new() { Id = Guid.NewGuid(), UserId = adminUser.Id, ClaimType = "Permission", ClaimValue = "FullAccess", CreatedOn = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), UserId = adminUser.Id, ClaimType = "Permission", ClaimValue = "ManageTemplates", CreatedOn = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), UserId = adminUser.Id, ClaimType = "Permission", ClaimValue = "ManageLookups", CreatedOn = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), UserId = adminUser.Id, ClaimType = "Permission", ClaimValue = "GenerateDocuments", CreatedOn = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), UserId = adminUser.Id, ClaimType = "Permission", ClaimValue = "ManageUsers", CreatedOn = DateTime.UtcNow }
            };
            await context.UserClaims.AddRangeAsync(adminClaims);
            await context.SaveChangesAsync();
        }
    }
}