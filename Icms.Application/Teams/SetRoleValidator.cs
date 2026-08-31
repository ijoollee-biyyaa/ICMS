using FluentValidation;
using Icms.Application.DTOs;

namespace Icms.Application.Teams;

public class SetRoleValidator : AbstractValidator<SetRoleRequest>
{
    public SetRoleValidator()
    {
        RuleFor(x => x.Role)
            .NotNull()
            .WithMessage("A role is required.");
    }
}