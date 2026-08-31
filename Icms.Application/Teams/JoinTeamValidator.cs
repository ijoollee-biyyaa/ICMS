using FluentValidation;
using Icms.Application.DTOs;

namespace Icms.Application.Teams;

public class JoinTeamValidator : AbstractValidator<JoinTeamRequest>
{
    public JoinTeamValidator()
    {
        RuleFor(x => x.MemberId)
            .NotNull()
            .WithMessage("A member is required.");
    }
}