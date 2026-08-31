using FluentValidation;
using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Churches;

public class UpdateChurchValidator : AbstractValidator<UpdateChurchRequest>
{
    public UpdateChurchValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Church name is required.")
            .MaximumLength(ChurchConstants.NameMaxLength)
                .WithMessage($"Church name must be {ChurchConstants.NameMaxLength} characters or fewer.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Church code is required.")
            .MaximumLength(ChurchConstants.CodeMaxLength)
                .WithMessage($"Church code must be {ChurchConstants.CodeMaxLength} characters or fewer.")
            .Matches(@"^[A-Z0-9-]{2,10}$")
                .WithMessage("Church code must contain only letters, digits, or dashes (e.g., BOLE-01).");

        RuleFor(x => x.Email)
            .MaximumLength(ChurchConstants.EmailMaxLength)
                .WithMessage($"Email must be {ChurchConstants.EmailMaxLength} characters or fewer.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email is invalid.");

        RuleFor(x => x.Phone)
            .MaximumLength(ChurchConstants.PhoneMaxLength)
                .WithMessage($"Phone must be {ChurchConstants.PhoneMaxLength} characters or fewer.");

        RuleFor(x => x.Tel)
            .MaximumLength(ChurchConstants.TelMaxLength)
                .WithMessage($"Tel must be {ChurchConstants.TelMaxLength} characters or fewer.");

        RuleFor(x => x.City)
            .MaximumLength(ChurchConstants.CityMaxLength)
                .WithMessage($"City must be {ChurchConstants.CityMaxLength} characters or fewer.");

        RuleFor(x => x.Subcity)
            .MaximumLength(ChurchConstants.SubcityMaxLength)
                .WithMessage($"Subcity must be {ChurchConstants.SubcityMaxLength} characters or fewer.");

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(ChurchConstants.WebsiteUrlMaxLength)
                .WithMessage($"Website URL must be {ChurchConstants.WebsiteUrlMaxLength} characters or fewer.");
    }
}