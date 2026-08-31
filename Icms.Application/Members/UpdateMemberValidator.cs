using FluentValidation;
using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Members;

public class UpdateMemberValidator : AbstractValidator<UpdateMemberRequest>
{
    public UpdateMemberValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(MemberConstants.NameMaxLength);

        RuleFor(x => x.FatherName)
            .NotEmpty()
            .WithMessage("Father's name is required.")
            .MaximumLength(MemberConstants.NameMaxLength);

        RuleFor(x => x.GrandfatherName)
            .NotEmpty()
            .WithMessage("Grandfather's name is required.")
            .MaximumLength(MemberConstants.NameMaxLength);

        RuleFor(x => x.DateOfBirth)
            .NotNull()
            .WithMessage("Date of birth is required.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob <= DateOnly.FromDateTime(DateTime.Today).AddYears(-MemberConstants.MinimumAge))
            .When(x => x.DateOfBirth is not null)
            .WithMessage($"Member must be at least {MemberConstants.MinimumAge} years old.");

        RuleFor(x => x.Phone)
            .MaximumLength(MemberConstants.PhoneMaxLength);

        RuleFor(x => x.Email)
            .MaximumLength(MemberConstants.EmailMaxLength)
            .EmailAddress()
            .When(x => x.Email is not null);

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(MemberConstants.PhotoUrlMaxLength);
    }
}