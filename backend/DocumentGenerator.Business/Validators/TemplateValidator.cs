using DocumentGenerator.Contracts.Models.Templates;
using FluentValidation;

namespace DocumentGenerator.Business.Validators;

public class TemplateValidator : AbstractValidator<TemplateSaveModel>
{
    public TemplateValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code can only contain letters, numbers, hyphens and underscores");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}