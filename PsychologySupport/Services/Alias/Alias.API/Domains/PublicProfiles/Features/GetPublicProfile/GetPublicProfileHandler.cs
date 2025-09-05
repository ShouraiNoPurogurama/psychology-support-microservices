using Alias.API.Data.Public;
using Alias.API.Domains.PublicProfiles.Dtos;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;

namespace Alias.API.Domains.PublicProfiles.Features.GetPublicProfile;

public record GetPublicProfileQuery(Guid AliasId) : IQuery<GetPublicProfileResult>;

public record GetPublicProfileResult(PublicProfileDto Profile);

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
                        .Include(a => a.AliasVersions)
                        .FirstOrDefaultAsync(a => a.Id == request.AliasId, cancellationToken: cancellationToken)
                    ?? throw new NotFoundException("Không tìm thấy hồ sơ.");

        var lastVersion = alias.AliasVersions
            .OrderByDescending(av => av.ValidFrom)
            .First();

        //TODO publish event aggregate follower, following, posts, avatar url sau khi triển khai xong các microservices đấy
        
        var dto = new PublicProfileDto(
            AliasId: alias.Id,
            Label: lastVersion.Label,
            Followers: 0,
            Followings: 0,
            Posts: 0,
            AvatarUrl: null,
            CreatedAt: alias.CreatedAt!.Value
            );

        return new GetPublicProfileResult(dto);
    }
}