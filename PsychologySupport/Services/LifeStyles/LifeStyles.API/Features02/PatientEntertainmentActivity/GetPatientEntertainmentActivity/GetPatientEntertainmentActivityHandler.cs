using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features02.PatientEntertainmentActivity.GetPatientEntertainmentActivity;

public record GetPatientEntertainmentActivityV2Query(Guid PatientProfileId)
    : IQuery<GetPatientEntertainmentActivityV2Result>;

public record GetPatientEntertainmentActivityV2Result(
    IEnumerable<Models.PatientEntertainmentActivity> PatientEntertainmentActivities
);

public class GetPatientEntertainmentActivityV2Handler(LifeStylesDbContext context)
    : IQueryHandler<GetPatientEntertainmentActivityV2Query, GetPatientEntertainmentActivityV2Result>
{
    public async Task<GetPatientEntertainmentActivityV2Result> Handle(
        GetPatientEntertainmentActivityV2Query request,
        CancellationToken cancellationToken)
    {
        var activities = await context.PatientEntertainmentActivities
            .Where(pea => pea.PatientProfileId == request.PatientProfileId)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
            throw new LifeStylesNotFoundException("Hoạt động giải trí", request.PatientProfileId);

        var activitiesDto = activities.Adapt<IEnumerable<Models.PatientEntertainmentActivity>>();

        return new GetPatientEntertainmentActivityV2Result(activitiesDto);
    }
}