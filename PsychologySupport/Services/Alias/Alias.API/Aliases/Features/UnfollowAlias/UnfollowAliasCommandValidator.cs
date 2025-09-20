/**
 * Unfollow Alias Command Validator
 * ===============================
 * 
 * Validates the UnfollowAliasCommand to ensure all required data is present
 * and meets the basic validation criteria before processing.
 */

using FluentValidation;

namespace Alias.API.Aliases.Features.UnfollowAlias;

/// <summary>
/// Validator for the UnfollowAliasCommand that ensures the command has valid data.
/// </summary>
public sealed class UnfollowAliasCommandValidator : AbstractValidator<UnfollowAliasCommand>
{
    public UnfollowAliasCommandValidator()
    {
        RuleFor(x => x.FollowedAliasId)
            .NotEmpty()
            .WithMessage("Followed alias ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Followed alias ID cannot be empty");
    }
}
