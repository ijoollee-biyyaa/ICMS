using FluentValidation;
using Icms.Application.DTOs;

namespace Icms.Application.Teams;

public class SaveAttendanceValidator : AbstractValidator<SaveAttendanceRequest>
{
    public SaveAttendanceValidator()
    {
        RuleFor(x => x.AttendanceDate)
            .NotNull()
            .WithMessage("An attendance date is required.")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("Attendance cannot be marked for a future date.");

        RuleFor(x => x.Entries)
            .NotNull()
            .WithMessage("Attendance entries are required.")
            .NotEmpty()
            .WithMessage("At least one attendance entry is required.");

        RuleFor(x => x.Entries)
            .Must(entries => entries!.GroupBy(e => e.MemberId).All(g => g.Count() == 1))
            .WithMessage("A member can appear only once in an attendance batch.")
            .When(x => x.Entries is { Count: > 0 });

        RuleForEach(x => x.Entries)
            .ChildRules(entry =>
            {
                entry.RuleFor(e => e.MemberId)
                    .NotNull()
                    .WithMessage("A member id is required for every attendance entry.");

                entry.RuleFor(e => e.Status)
                    .NotNull()
                    .WithMessage("A status is required for every attendance entry.");

                entry.RuleFor(e => e.Reason)
                    .MaximumLength(200)
                    .WithMessage("Absence reason cannot exceed 200 characters.");
            })
            .When(x => x.Entries is { Count: > 0 });
    }
}