/**
 * Get Following Query Validator
 * ============================
 * 
 * Validates the GetFollowingQuery to ensure pagination parameters are within acceptable ranges
 * and the alias ID is valid.
 */

using FluentValidation;

namespace Alias.API.Aliases.Features.GetFollowing;

/// <summary>
/// Validator for GetFollowingQuery that ensures valid pagination and alias ID.
/// </summary>
public sealed class GetFollowingQueryValidator : AbstractValidator<GetFollowingQuery>
{
    public GetFollowingQueryValidator()
    {
        RuleFor(x => x.AliasId)
            .NotEmpty()
            .WithMessage("Alias ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Alias ID cannot be empty");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page number must be 0 or greater");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");
    }
}
