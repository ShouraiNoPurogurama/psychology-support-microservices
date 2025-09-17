using FluentValidation;

namespace Alias.API.Aliases.Features.RenameAlias;

public sealed class RenameAliasCommandValidator : AbstractValidator<RenameAliasCommand>
{
    public RenameAliasCommandValidator()
    {
        RuleFor(x => x.SubjectRef)
            .NotEmpty()
            .WithMessage("Reference không được để trống.");

        RuleFor(x => x.NewLabel)
            .NotEmpty()
            .WithMessage("Tên bí danh không được để trống.")
            .MaximumLength(30)
            .WithMessage("Tên bí danh tối đa 30 kí tự.")
            .Matches(@"^[a-zA-Z0-9_.-]+$")
            .WithMessage("Bí danh chỉ được chứa chữ cái, số, dấu gạch dưới, gạch ngang và dấu chấm.");
    }
}