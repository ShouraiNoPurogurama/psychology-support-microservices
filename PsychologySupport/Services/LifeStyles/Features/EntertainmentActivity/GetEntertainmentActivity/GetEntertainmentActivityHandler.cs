using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.EntertainmentActivity.GetEntertainmentActivity;
public record GetEntertainmentActivityQuery(Guid Id) : IQuery<GetEntertainmentActivityResult>;

public record GetEntertainmentActivityResult(EntertainmentActivityDto EntertainmentActivity);

    public class GetEntertainmentActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetEntertainmentActivityQuery, GetEntertainmentActivityResult>
    {
        public async Task<GetEntertainmentActivityResult> Handle(GetEntertainmentActivityQuery request, CancellationToken cancellationToken)
        {
            var activity = await context.EntertainmentActivities
                .FirstOrDefaultAsync(ea => ea.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("Entertainment Activity", request.Id);

            var activityDto = activity.Adapt<EntertainmentActivityDto>();

            return new GetEntertainmentActivityResult(activityDto);
        }
    }

