namespace DocumentGenerator.DAL.Entities;

public class RoleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }

    // Navigation properties
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
    public ICollection<RoleClaimEntity> RoleClaims { get; set; } = new List<RoleClaimEntity>();
}
