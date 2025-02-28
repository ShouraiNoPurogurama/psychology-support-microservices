using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.TherapeuticActivity.GetTherapeuticActivity;

public record GetTherapeuticActivityQuery(Guid Id) : IQuery<GetTherapeuticActivityResult>;

public record GetTherapeuticActivityResult(TherapeuticActivityDto TherapeuticActivity);

public class GetTherapeuticActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetTherapeuticActivityQuery, GetTherapeuticActivityResult>
{
    public async Task<GetTherapeuticActivityResult> Handle(GetTherapeuticActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activity = await context.TherapeuticActivities
                           .Include(ta => ta.TherapeuticType) // Load TherapeuticType để lấy Name
                           .FirstOrDefaultAsync(ta => ta.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException("Therapeutic Activity", request.Id);

        var activityDto = activity.Adapt<TherapeuticActivityDto>();

        return new GetTherapeuticActivityResult(activityDto);
    }
}