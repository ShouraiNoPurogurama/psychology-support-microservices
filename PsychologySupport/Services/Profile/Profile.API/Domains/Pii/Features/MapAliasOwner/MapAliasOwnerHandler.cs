using BuildingBlocks.Utils;
using Profile.API.Data.Pii;
using Profile.API.Models.Pii;

namespace Profile.API.Domains.Pii.Features.MapAliasOwner;

public record MapAliasOwnerCommand(Guid AliasId, Guid SubjectRef) : ICommand<MapAliasOwnerResult>;

public record MapAliasOwnerResult(Guid OwnerId);

public class MapAliasOwnerHandler(PiiDbContext dbContext, ILogger<MapAliasOwnerHandler> logger)
    : ICommandHandler<MapAliasOwnerCommand, MapAliasOwnerResult>
{
    public async Task<MapAliasOwnerResult> Handle(MapAliasOwnerCommand request, CancellationToken cancellationToken)
    {
        if (request.AliasId == Guid.Empty || request.SubjectRef == Guid.Empty)
            throw new BadRequestException("Id bí danh hoặc reference không hợp lệ.");

        var existed = dbContext.AliasOwnerMaps
            .AsNoTracking()
            .Where(a => a.AliasId == request.AliasId || a.SubjectRef == request.SubjectRef)
            .Select(x => x.Id)
            .FirstOrDefault();

        if (existed != Guid.Empty)
            return new MapAliasOwnerResult(existed);

        var newMapping = new AliasOwnerMap
        {
            Id = Guid.NewGuid(),
            AliasId = request.AliasId,
            SubjectRef = request.SubjectRef
        };

        await dbContext.AliasOwnerMaps.AddAsync(newMapping, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new MapAliasOwnerResult(newMapping.Id);
        }
        catch (DbUpdateException ex) when (DbUtils.IsUniqueViolation(ex))
        {
            var existedOwnerId = await dbContext.AliasOwnerMaps
                .AsNoTracking()
                .Where(a => a.AliasId == request.AliasId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existedOwnerId != Guid.Empty)
            {
                logger.LogInformation("AliasOwnerMaps unique hit, returning existing. alias={AliasId}", request.AliasId);
                return new MapAliasOwnerResult(existedOwnerId);
            }

            throw;
        }
    }
}