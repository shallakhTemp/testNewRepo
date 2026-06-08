namespace DocumentGenerator.DAL.Entities;

public class RoleClaimEntity
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }

    // Navigation properties
    public RoleEntity Role { get; set; } = null!;
}
