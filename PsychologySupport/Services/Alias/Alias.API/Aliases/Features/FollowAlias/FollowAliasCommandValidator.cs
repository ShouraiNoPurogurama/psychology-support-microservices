using FluentValidation;

namespace Alias.API.Aliases.Features.FollowAlias;

/// <summary>
/// Validator for the FollowAliasCommand that ensures the command has valid data.
/// </summary>
public sealed class FollowAliasCommandValidator : AbstractValidator<FollowAliasCommand>
{
    public FollowAliasCommandValidator()
    {
        RuleFor(x => x.FollowedAliasId)
            .NotEmpty()
            .WithMessage("Followed alias ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Followed alias ID cannot be empty");
    }
}
