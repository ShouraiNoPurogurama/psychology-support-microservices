using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Translation;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features02.EntertainmentActivity.GetEntertainmentActivity;

public record GetEntertainmentActivityV2Query(Guid Id)
    : IQuery<GetEntertainmentActivityV2Result>;

public record GetEntertainmentActivityV2Result(EntertainmentActivityDto EntertainmentActivity);

public class GetEntertainmentActivityV2Handler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient)
    : IQueryHandler<GetEntertainmentActivityV2Query, GetEntertainmentActivityV2Result>
{
    public async Task<GetEntertainmentActivityV2Result> Handle(
        GetEntertainmentActivityV2Query request,
        CancellationToken cancellationToken)
    {
        var activity = await context.EntertainmentActivities
                           .FirstOrDefaultAsync(ea => ea.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException("Entertainment Activity", request.Id);

        var activityDto = activity.Adapt<EntertainmentActivityDto>();

        var translatedActivity = await activityDto.TranslateEntityAsync(
            nameof(EntertainmentActivity),
            translationClient,
            cancellationToken,
            ea => ea.Name,
            ea => ea.Description,
            ea => ea.IntensityLevel,
            ea => ea.ImpactLevel);

        return new GetEntertainmentActivityV2Result(translatedActivity);
    }
}