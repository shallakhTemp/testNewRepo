namespace DocumentGenerator.DAL.Entities;

public class UserClaimEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;
}
