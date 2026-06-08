using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Models.Auth;
using DocumentGenerator.Contracts.Resources.Auth;
using DocumentGenerator.DAL.Entities;
using DocumentGenerator.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DocumentGenerator.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResource> LoginAsync(LoginModel model)
    {
        var user = await _unitOfWork.Users.GetQueryable()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserClaims)
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var claims = user.UserClaims.Select(uc => uc.ClaimValue).ToList();

        var token = GenerateJwtToken(user, roles, claims);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken.Token;
        user.RefreshTokenExpiryTime = refreshToken.ExpiryTime;
        await _unitOfWork.CompleteAsync();

        return new AuthResource
        {
            AccessToken = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiryTime,
            User = new UserResource
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles,
                Claims = claims
            }
        };
    }

    public async Task<AuthResource> RefreshTokenAsync(RefreshTokenModel model)
    {
        var principal = GetPrincipalFromExpiredToken(model.AccessToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null || !Guid.TryParse(userId, out var id))
            throw new UnauthorizedAccessException("Invalid token");

        var user = await _unitOfWork.Users.GetByIdAsync(id);

        if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var roles = (await _unitOfWork.UserRoles.GetQueryable()
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == id)
            .Select(ur => ur.Role.Name)
            .ToListAsync())!;

        var claims = (await _unitOfWork.UserClaims.GetQueryable()
            .Where(uc => uc.UserId == id)
            .Select(uc => uc.ClaimValue)
            .ToListAsync())!;

        var token = GenerateJwtToken(user, roles, claims);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken.Token;
        user.RefreshTokenExpiryTime = refreshToken.ExpiryTime;
        await _unitOfWork.CompleteAsync();

        return new AuthResource
        {
            AccessToken = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiryTime,
            User = new UserResource
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles,
                Claims = claims
            }
        };
    }

    public async Task<UserResource> GetUserByIdAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetQueryable()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        return new UserResource
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Claims = user.UserClaims.Select(uc => uc.ClaimValue).ToList()
        };
    }

    private string GenerateJwtToken(UserEntity user, List<string> roles, List<string> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };

        claimsList.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claimsList.AddRange(claims.Select(c => new Claim("Permission", c)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claimsList,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static (string Token, DateTime ExpiryTime) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return (Convert.ToBase64String(randomBytes), DateTime.UtcNow.AddDays(7));
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}