using FluentValidation;

namespace Alias.API.Aliases.Features.IssueAlias;

public sealed class IssueAliasCommandValidator : AbstractValidator<IssueAliasCommand>
{
    public IssueAliasCommandValidator()
    {
        RuleFor(x => x.SubjectRef)
            .NotEmpty()
            .WithMessage("Reference không được để trống.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Bí danh không được để trống.")
            .MaximumLength(30)
            .WithMessage("Bí danh tối đa 30 kí tự.")
            .Matches(@"^[a-zA-Z0-9_.-]+$")
            .WithMessage("Bí danh chỉ được chứa chữ cái, số, dấu gạch dưới, gạch ngang và dấu chấm.");
    }
}