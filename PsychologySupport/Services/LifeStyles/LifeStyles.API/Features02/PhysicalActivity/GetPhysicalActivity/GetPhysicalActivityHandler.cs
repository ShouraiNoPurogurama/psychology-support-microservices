using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Translation;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features02.PhysicalActivity.GetPhysicalActivity;

public record GetPhysicalActivityV2Query(Guid Id) : IQuery<GetPhysicalActivityV2Result>;

public record GetPhysicalActivityV2Result(PhysicalActivityDto PhysicalActivity);

public class GetPhysicalActivityV2Handler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient)
    : IQueryHandler<GetPhysicalActivityV2Query, GetPhysicalActivityV2Result>
{
    public async Task<GetPhysicalActivityV2Result> Handle(GetPhysicalActivityV2Query request, CancellationToken cancellationToken)
    {
        var activity = await context.PhysicalActivities
                           .FirstOrDefaultAsync(pa => pa.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException("Physical Activity", request.Id);

        var activityDto = activity.Adapt<PhysicalActivityDto>();

        var translatedDto = await activityDto.TranslateEntityAsync(
            nameof(Models.PhysicalActivity),
            translationClient,
            cancellationToken,
            a => a.Name,
            a => a.Description,
            a => a.IntensityLevel,
            a => a.ImpactLevel
        );

        return new GetPhysicalActivityV2Result(translatedDto);
    }
}