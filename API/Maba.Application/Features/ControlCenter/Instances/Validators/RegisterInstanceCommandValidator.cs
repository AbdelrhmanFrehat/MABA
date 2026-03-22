using FluentValidation;
using Maba.Application.Features.ControlCenter.Instances.Commands;

namespace Maba.Application.Features.ControlCenter.Instances.Validators;

public class RegisterInstanceCommandValidator : AbstractValidator<RegisterInstanceCommand>
{
    public RegisterInstanceCommandValidator()
    {
        RuleFor(x => x.OrgId).NotEmpty();
        RuleFor(x => x.Hostname).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CoreVersion).NotEmpty().MaximumLength(50);
    }
}

