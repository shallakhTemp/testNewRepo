namespace DocumentGenerator.Contracts.Resources.Auth;

public class UserResource
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Claims { get; set; } = new();
}
