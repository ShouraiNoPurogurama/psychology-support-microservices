using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Translation;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features02.TherapeuticActivity.GetTherapeuticActivity;

public record GetTherapeuticActivityV2Query(Guid Id)
    : IQuery<GetTherapeuticActivityV2Result>;

public record GetTherapeuticActivityV2Result(TherapeuticActivityDto TherapeuticActivity);

public class GetTherapeuticActivityV2Handler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient
) : IQueryHandler<GetTherapeuticActivityV2Query, GetTherapeuticActivityV2Result>
{
    public async Task<GetTherapeuticActivityV2Result> Handle(
        GetTherapeuticActivityV2Query request,
        CancellationToken cancellationToken)
    {
        var activity = await context.TherapeuticActivities
            .Include(ta => ta.TherapeuticType)
            .FirstOrDefaultAsync(ta => ta.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Hoạt động Thiền Định", request.Id);

        var activityDto = new TherapeuticActivityDto(
            activity.Id,
            activity.TherapeuticType.Name,
            activity.Name,
            activity.Description,
            activity.Instructions,
            activity.IntensityLevel.ToReadableString(),
            activity.ImpactLevel.ToReadableString()
        );

        var translatedDto = await activityDto.TranslateEntityAsync(
            nameof(Models.TherapeuticActivity),
            translationClient,
            cancellationToken,
            a => a.TherapeuticTypeName,
            a => a.Name,
            a => a.Description,
            a => a.Instructions,
            a => a.IntensityLevel,
            a => a.ImpactLevel
        );

        return new GetTherapeuticActivityV2Result(translatedDto);
    }
}
