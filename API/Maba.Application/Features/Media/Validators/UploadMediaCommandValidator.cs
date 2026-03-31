using FluentValidation;
using Maba.Application.Features.Media.Commands;

namespace Maba.Application.Features.Media.Validators;

public class UploadMediaCommandValidator : AbstractValidator<UploadMediaCommand>
{
    public UploadMediaCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required");

        RuleFor(x => x.MediaTypeId)
            .NotEmpty().WithMessage("Media type ID is required");

        RuleFor(x => x.File)
            .Must(f => f != null && f.Length > 0).WithMessage("File cannot be empty")
            .When(x => x.File != null);
    }
}

