using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Translation;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.EntertainmentActivity.GetEntertainmentActivity;

public record GetEntertainmentActivityQuery(Guid Id) : IQuery<GetEntertainmentActivityResult>;

public record GetEntertainmentActivityResult(EntertainmentActivityDto EntertainmentActivity);

public class GetEntertainmentActivityHandler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient)
    : IQueryHandler<GetEntertainmentActivityQuery, GetEntertainmentActivityResult>
{
    public async Task<GetEntertainmentActivityResult> Handle(GetEntertainmentActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activity = await context.EntertainmentActivities
                           .FirstOrDefaultAsync(ea => ea.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException("Entertainment Activity", request.Id);

        var activityDto = activity.Adapt<EntertainmentActivityDto>();

        var translatedActivity = await activityDto.TranslateEntityAsync(nameof(EntertainmentActivity), translationClient,
            cancellationToken,
            ea => ea.Name, ea => ea.Description, ea => ea.IntensityLevel, ea => ea.ImpactLevel);
        
        return new GetEntertainmentActivityResult(translatedActivity);
    }
}