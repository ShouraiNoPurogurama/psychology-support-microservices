using MassTransit;

namespace Feed.Application.Features.Ranking;

public record UpdatePostRankCommand(Guid PostId);

