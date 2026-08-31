using FluentValidation;
using Icms.Application.DTOs;

namespace Icms.Application.Teams;

public class RecordPaymentValidator : AbstractValidator<RecordPaymentRequest>
{
    public RecordPaymentValidator()
    {
        RuleFor(x => x.MemberId)
            .NotNull()
            .WithMessage("A member id is required.");

        RuleFor(x => x.Month)
            .NotNull()
            .WithMessage("A month is required.")
            .Must(month => month!.Value.Day == 1)
            .WithMessage("Month must be the first day of the month (e.g. 2026-08-01).")
            .Must(month => month <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("A payment cannot be recorded for a future month.");

        RuleFor(x => x.Amount)
            .NotNull()
            .WithMessage("An amount is required.")
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .Must(v => v is null || decimal.Round(v.Value, 2) == v.Value)
            .WithMessage("Amount can have at most 2 decimal places.");
    }
}

public class UpdatePaymentValidator : AbstractValidator<UpdatePaymentRequest>
{
    public UpdatePaymentValidator()
    {
        RuleFor(x => x.Amount)
            .NotNull()
            .WithMessage("An amount is required.")
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .Must(v => v is null || decimal.Round(v.Value, 2) == v.Value)
            .WithMessage("Amount can have at most 2 decimal places.");
    }
}