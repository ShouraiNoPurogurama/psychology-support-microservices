using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientPhysicalActivity.GetPatientPhysicalActivity;

public record GetPatientPhysicalActivityQuery(Guid PatientProfileId)
    : IQuery<GetPatientPhysicalActivityResult>;

public record GetPatientPhysicalActivityResult(IEnumerable<Models.PatientPhysicalActivity> PatientPhysicalActivities);

public class GetPatientPhysicalActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetPatientPhysicalActivityQuery, GetPatientPhysicalActivityResult>
{
    public async Task<GetPatientPhysicalActivityResult> Handle(GetPatientPhysicalActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activities = await context.PatientPhysicalActivities
            .Where(ppa => ppa.PatientProfileId == request.PatientProfileId)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
            throw new LifeStylesNotFoundException("Patient Physical Activity", request.PatientProfileId);

        var activitiesDto = activities.Adapt<IEnumerable<Models.PatientPhysicalActivity>>();

        return new GetPatientPhysicalActivityResult(activitiesDto);
    }
}