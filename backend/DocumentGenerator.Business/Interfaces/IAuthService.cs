using DocumentGenerator.Contracts.Models.Auth;
using DocumentGenerator.Contracts.Resources.Auth;

namespace DocumentGenerator.Business.Interfaces;

public interface IAuthService
{
    Task<AuthResource> LoginAsync(LoginModel model);
    Task<AuthResource> RefreshTokenAsync(RefreshTokenModel model);
    Task<UserResource> GetUserByIdAsync(Guid userId);
}