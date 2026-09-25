using FluentValidation;
using Icms.Application.DTOs;
using Icms.Domain.Enums;

namespace Icms.Application.Members;

public class InitiateTransferValidator : AbstractValidator<InitiateTransferRequest>
{
    public InitiateTransferValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        
        When(x => x.Type == TransferType.Internal, () =>
        {
            RuleFor(x => x.DestinationChurchId)
                .NotNull()
                .WithMessage("Destination church must be selected for internal transfers.");
        });

        When(x => x.Type != TransferType.Internal, () =>
        {
            RuleFor(x => x.DestinationChurchName)
                .NotEmpty()
                .WithMessage("Destination church name is required for external transfers.");
        });
    }
}

public class RegisterExternalIncomingValidator : AbstractValidator<RegisterExternalIncomingRequest>
{
    public RegisterExternalIncomingValidator()
    {
        RuleFor(x => x.DestinationChurchId).GreaterThan(0);
        RuleFor(x => x.SourceChurchName).NotEmpty();
        RuleFor(x => x.IncomingFirstName).NotEmpty();
        RuleFor(x => x.IncomingFatherName).NotEmpty();
        RuleFor(x => x.IncomingGrandfatherName).NotEmpty();
    }
}
