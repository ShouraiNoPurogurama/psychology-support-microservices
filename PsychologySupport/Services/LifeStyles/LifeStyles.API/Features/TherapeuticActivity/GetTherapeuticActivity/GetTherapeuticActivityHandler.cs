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

namespace LifeStyles.API.Features.TherapeuticActivity.GetTherapeuticActivity;

public record GetTherapeuticActivityQuery(Guid Id) : IQuery<GetTherapeuticActivityResult>;

public record GetTherapeuticActivityResult(TherapeuticActivityDto TherapeuticActivity);

public class GetTherapeuticActivityHandler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient)
    : IQueryHandler<GetTherapeuticActivityQuery, GetTherapeuticActivityResult>
{
    public async Task<GetTherapeuticActivityResult> Handle(GetTherapeuticActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activity = await context.TherapeuticActivities
            .Include(ta => ta.TherapeuticType)
            .FirstOrDefaultAsync(ta => ta.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Hoạt động Thiền Định", request.Id);

        //Mapping thủ công để có TherapeuticTypeName
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

        return new GetTherapeuticActivityResult(translatedDto);
    }
}
