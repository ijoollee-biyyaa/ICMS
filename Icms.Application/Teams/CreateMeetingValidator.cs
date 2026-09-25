using FluentValidation;
using Icms.Application.DTOs;

namespace Icms.Application.Teams;

public class CreateMeetingValidator : AbstractValidator<CreateMeetingRequest>
{
    public CreateMeetingValidator()
    {
        RuleFor(x => x.MeetingDate)
            .NotNull()
            .WithMessage("A meeting date is required.")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("A meeting cannot be scheduled for a future date.");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Meeting title cannot exceed 200 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Meeting notes cannot exceed 500 characters.");
    }
}