using System.ComponentModel.DataAnnotations;

namespace DocumentGenerator.Contracts.Models.Auth;

public class RefreshTokenModel
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
