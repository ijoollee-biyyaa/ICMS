using FluentValidation;
using Icms.Application.DTOs;

namespace Icms.Application.Districts;

public class CreateDistrictValidator : AbstractValidator<CreateDistrictRequest>
{
    public CreateDistrictValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("A district name is required.")
            .NotEmpty().WithMessage("A district name is required.")
            .MaximumLength(100).WithMessage("District name can be at most 100 characters.");

        RuleFor(x => x.Code)
            .NotNull().WithMessage("A district code is required.")
            .NotEmpty().WithMessage("A district code is required.")
            .MaximumLength(20).WithMessage("District code can be at most 20 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address can be at most 200 characters.");
    }
}

public class UpdateDistrictValidator : AbstractValidator<UpdateDistrictRequest>
{
    public UpdateDistrictValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("A district name is required.")
            .NotEmpty().WithMessage("A district name is required.")
            .MaximumLength(100).WithMessage("District name can be at most 100 characters.");

        RuleFor(x => x.Code)
            .NotNull().WithMessage("A district code is required.")
            .NotEmpty().WithMessage("A district code is required.")
            .MaximumLength(20).WithMessage("District code can be at most 20 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address can be at most 200 characters.");
    }
}