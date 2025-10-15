using Alias.API.Aliases.Dtos;
using Alias.API.Aliases.Exceptions;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using FluentValidation;

namespace Alias.API.Aliases.Features.GetPreferences;

public record GetPreferencesQuery(Guid AliasId) : IQuery<GetPreferencesResult>;

public record GetPreferencesResult(UserPreferencesDto Preferences);

public sealed class GetPreferencesQueryValidator : AbstractValidator<GetPreferencesQuery>
{
    public GetPreferencesQueryValidator()
    {
        RuleFor(x => x.AliasId)
            .NotEmpty()
            .WithMessage("Alias ID is required");
    }
}

public class GetPreferencesHandler : IQueryHandler<GetPreferencesQuery, GetPreferencesResult>
{
    private readonly AliasDbContext _dbContext;

    public GetPreferencesHandler(AliasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetPreferencesResult> Handle(GetPreferencesQuery request, CancellationToken cancellationToken)
    {
        var alias = await _dbContext.Aliases
                        .AsNoTracking()
                        .Where(a => a.Id == request.AliasId && !a.IsDeleted)
                        .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new AliasNotFoundException();

        var preferencesDto = new UserPreferencesDto(
            Theme: alias.Preferences.Theme,
            Language: alias.Preferences.Language,
            NotificationsEnabled: alias.Preferences.NotificationsEnabled
        );

        return new GetPreferencesResult(preferencesDto);
    }
}
