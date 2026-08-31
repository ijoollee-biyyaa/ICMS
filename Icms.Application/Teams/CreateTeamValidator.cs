using FluentValidation;
using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Teams;

public class CreateTeamValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.MembershipRule)
            .NotNull()
            .When(x => x.ParentTeamId is null)
            .WithMessage("A membership rule is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Team name is required.")
            .MaximumLength(TeamConstants.NameMaxLength);
    }
}