using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Translation;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PhysicalActivity.GetPhysicalActivity;

public record GetPhysicalActivityQuery(Guid Id) : IQuery<GetPhysicalActivityResult>;

public record GetPhysicalActivityResult(PhysicalActivityDto PhysicalActivity);

public class GetPhysicalActivityHandler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient)
    : IQueryHandler<GetPhysicalActivityQuery, GetPhysicalActivityResult>
{
    public async Task<GetPhysicalActivityResult> Handle(GetPhysicalActivityQuery request, CancellationToken cancellationToken)
    {
        var activity = await context.PhysicalActivities
                           .FirstOrDefaultAsync(pa => pa.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException("Hoạt động Thể Chất", request.Id);

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

        return new GetPhysicalActivityResult(translatedDto);
    }
}