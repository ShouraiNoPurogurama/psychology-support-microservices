using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.GetPatientEntertainmentActivity;

public record GetPatientEntertainmentActivityQuery(Guid PatientProfileId)
    : IQuery<GetPatientEntertainmentActivityResult>;

public record GetPatientEntertainmentActivityResult(
    IEnumerable<Models.PatientEntertainmentActivity> PatientEntertainmentActivities);

public class GetPatientEntertainmentActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetPatientEntertainmentActivityQuery, GetPatientEntertainmentActivityResult>
{
    public async Task<GetPatientEntertainmentActivityResult> Handle(GetPatientEntertainmentActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activities = await context.PatientEntertainmentActivities
            .Where(pea => pea.PatientProfileId == request.PatientProfileId)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
            throw new LifeStylesNotFoundException("Patient Entertainment Activity", request.PatientProfileId);

        var activitiesDto = activities.Adapt<IEnumerable<Models.PatientEntertainmentActivity>>();

        return new GetPatientEntertainmentActivityResult(activitiesDto);
    }
}