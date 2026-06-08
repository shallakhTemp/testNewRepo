using DocumentGenerator.Contracts.Models.Lookups;
using FluentValidation;

namespace DocumentGenerator.Business.Validators;

public class LookupValidator : AbstractValidator<LookupModel>
{
    public LookupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}

public class LookupItemValidator : AbstractValidator<LookupItemModel>
{
    public LookupItemValidator()
    {
        RuleFor(x => x.LookupId)
            .NotEmpty().WithMessage("Lookup ID is required");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required")
            .MaximumLength(200).WithMessage("Value must not exceed 200 characters");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(200).WithMessage("Text must not exceed 200 characters");
    }
}

