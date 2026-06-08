using DocumentGenerator.Contracts.Models.Auth;
using FluentValidation;

namespace DocumentGenerator.Business.Validators;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenModel>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}