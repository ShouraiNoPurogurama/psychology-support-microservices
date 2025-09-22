using Alias.API.Aliases.Dtos;
using Alias.API.Aliases.Exceptions;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using FluentValidation;

namespace Alias.API.Aliases.Features.GetPublicProfile;

public record GetPublicProfileQuery(Guid AliasId) : IQuery<GetPublicProfileResult>;

public record GetPublicProfileResult(PublicProfileDto Profile);

public sealed class GetPublicProfileQueryValidator : AbstractValidator<GetPublicProfileQuery>
{
    public GetPublicProfileQueryValidator()
    {
        RuleFor(x => x.AliasId)
            .NotEmpty()
            .WithMessage("Alias ID is required");
    }
}

public class GetPublicProfileHandler : IQueryHandler<GetPublicProfileQuery, GetPublicProfileResult>
{
    private readonly AliasDbContext _dbContext;

    public GetPublicProfileHandler(AliasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetPublicProfileResult> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
    {
        var alias = await _dbContext.Aliases
                        .AsNoTracking()
                        .Where(a => a.Id == request.AliasId && !a.IsDeleted)
                        .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new AliasNotFoundException();
        
        var lastVersion = alias.Versions.OrderByDescending(v => v.CreatedAt).First();

        var dto = new PublicProfileDto(
            AliasId: alias.Id,
            Label: lastVersion.DisplayName,
            Followers: 0,
            Followings: 0,
            Posts: 0,
            AvatarUrl: alias.AvatarMediaId.ToString(),
            CreatedAt: alias.CreatedAt
        );

        return new GetPublicProfileResult(dto);
    }
}