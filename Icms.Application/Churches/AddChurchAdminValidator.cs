using FluentValidation;
using Icms.Application.DTOs;

namespace Icms.Application.Churches;

public class AddChurchAdminValidator : AbstractValidator<AddChurchAdminRequest>
{
    public AddChurchAdminValidator()
    {
        RuleFor(x => x)
            .Must(x => x.MemberId.HasValue
                || (!string.IsNullOrWhiteSpace(x.FirstName)
                    && !string.IsNullOrWhiteSpace(x.FatherName)
                    && !string.IsNullOrWhiteSpace(x.GrandfatherName)))
            .WithMessage("Provide a member id, or the full name of the new admin.");

        RuleFor(x => x.FirstName)
            .NotEmpty().When(x => !x.MemberId.HasValue)
            .WithMessage("First name is required for a new admin.");

        RuleFor(x => x.FatherName)
            .NotEmpty().When(x => !x.MemberId.HasValue)
            .WithMessage("Father name is required for a new admin.");

        RuleFor(x => x.GrandfatherName)
            .NotEmpty().When(x => !x.MemberId.HasValue)
            .WithMessage("Grandfather name is required for a new admin.");
    }
}