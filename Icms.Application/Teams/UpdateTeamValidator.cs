using FluentValidation;
using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Teams;

public class UpdateTeamValidator : AbstractValidator<UpdateTeamRequest>
{
    public UpdateTeamValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Team name is required.")
            .MaximumLength(TeamConstants.NameMaxLength);
    }
}