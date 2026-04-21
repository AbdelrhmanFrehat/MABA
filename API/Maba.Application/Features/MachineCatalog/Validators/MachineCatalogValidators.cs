using FluentValidation;
using Maba.Application.Features.MachineCatalog.DTOs;
using Maba.Application.Features.MachineCatalog.Handlers;

namespace Maba.Application.Features.MachineCatalog.Validators;

public class CreateMachineCategoryCommandValidator : AbstractValidator<CreateMachineCategoryCommand>
{
    public CreateMachineCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(100).WithMessage("Code must not exceed 100 characters.");

        RuleFor(x => x.Request.DisplayNameEn)
            .NotEmpty().WithMessage("English display name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Request.DisplayNameAr)
            .NotEmpty().WithMessage("Arabic display name is required.")
            .MaximumLength(200);
    }
}

public class UpdateMachineCategoryCommandValidator : AbstractValidator<UpdateMachineCategoryCommand>
{
    public UpdateMachineCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.DisplayNameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.DisplayNameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateMachineFamilyCommandValidator : AbstractValidator<CreateMachineFamilyCommand>
{
    public CreateMachineFamilyCommandValidator()
    {
        RuleFor(x => x.Request.CategoryId).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.DisplayNameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.DisplayNameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Manufacturer).NotEmpty().MaximumLength(200);
    }
}

public class UpdateMachineFamilyCommandValidator : AbstractValidator<UpdateMachineFamilyCommand>
{
    public UpdateMachineFamilyCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.CategoryId).NotEmpty();
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.DisplayNameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.DisplayNameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Manufacturer).NotEmpty().MaximumLength(200);
    }
}

public class CreateMachineDefinitionCommandValidator : AbstractValidator<CreateMachineDefinitionCommand>
{
    public CreateMachineDefinitionCommandValidator()
    {
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Version).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.CategoryId).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.Request.FamilyId).NotEmpty().WithMessage("Family is required.");
        RuleFor(x => x.Request.DisplayNameEn).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.DisplayNameAr).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.Manufacturer).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Request.RuntimeBinding)
            .NotNull().WithMessage("RuntimeBinding section is required.");

        RuleFor(x => x.Request.RuntimeBinding.DefaultDriverType)
            .Must((cmd, dt) => cmd.Request.RuntimeBinding.SupportedDriverTypes.Contains(dt))
            .WithMessage("defaultDriverType must be included in supportedDriverTypes.");

        RuleFor(x => x.Request.RuntimeBinding.SupportedDriverTypes)
            .NotEmpty().WithMessage("At least one supported driver type is required.");

        RuleFor(x => x.Request.RuntimeBinding.SupportedSetupModes)
            .NotEmpty().WithMessage("At least one supported setup mode is required.");

        RuleFor(x => x.Request.AxisConfig.AxisCount)
            .GreaterThan(0).WithMessage("axisCount must be greater than 0.")
            .Must((cmd, count) => cmd.Request.AxisConfig.SupportedAxes.Count == count)
            .WithMessage("axisCount must match the count of supportedAxes.");

        RuleFor(x => x.Request.ConnectionDefaults.DefaultBaudRate)
            .Must((cmd, baud) => cmd.Request.ConnectionDefaults.SupportedBaudRates.Contains(baud))
            .WithMessage("defaultBaudRate must be included in supportedBaudRates.");

        RuleFor(x => x.Request.MotionDefaults.JogPresets)
            .NotEmpty().WithMessage("At least one jog preset is required.");

        RuleFor(x => x.Request.Workspace.WorkAreaMm)
            .NotNull().WithMessage("workAreaMm is required.");

        RuleFor(x => x.Request.ProfileRules)
            .NotNull().WithMessage("ProfileRules section is required.");
    }
}

public class UpdateMachineDefinitionCommandValidator : AbstractValidator<UpdateMachineDefinitionCommand>
{
    public UpdateMachineDefinitionCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Version).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.CategoryId).NotEmpty();
        RuleFor(x => x.Request.FamilyId).NotEmpty();
        RuleFor(x => x.Request.DisplayNameEn).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.DisplayNameAr).NotEmpty().MaximumLength(300);

        RuleFor(x => x.Request.RuntimeBinding.DefaultDriverType)
            .Must((cmd, dt) => cmd.Request.RuntimeBinding.SupportedDriverTypes.Contains(dt))
            .WithMessage("defaultDriverType must be included in supportedDriverTypes.");

        RuleFor(x => x.Request.AxisConfig.AxisCount)
            .Must((cmd, count) => cmd.Request.AxisConfig.SupportedAxes.Count == count)
            .WithMessage("axisCount must match the count of supportedAxes.");

        RuleFor(x => x.Request.ConnectionDefaults.DefaultBaudRate)
            .Must((cmd, baud) => cmd.Request.ConnectionDefaults.SupportedBaudRates.Contains(baud))
            .WithMessage("defaultBaudRate must be included in supportedBaudRates.");
    }
}
