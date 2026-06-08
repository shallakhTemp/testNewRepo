namespace DocumentGenerator.DAL.Entities;

public class UserRoleEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedOn { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;
    public RoleEntity Role { get; set; } = null!;
}
