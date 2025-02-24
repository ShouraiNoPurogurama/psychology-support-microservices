using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PhysicalActivity.GetPhysicalActivity;

public record GetPhysicalActivityQuery(Guid Id) : IQuery<GetPhysicalActivityResult>;

public record GetPhysicalActivityResult(PhysicalActivityDto PhysicalActivity);

public class GetPhysicalActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetPhysicalActivityQuery, GetPhysicalActivityResult>
{
    public async Task<GetPhysicalActivityResult> Handle(GetPhysicalActivityQuery request, CancellationToken cancellationToken)
    {
        var activity = await context.PhysicalActivities
            .FirstOrDefaultAsync(pa => pa.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Physical Activity", request.Id);

        var activityDto = activity.Adapt<PhysicalActivityDto>();

        return new GetPhysicalActivityResult(activityDto);
    }
}
