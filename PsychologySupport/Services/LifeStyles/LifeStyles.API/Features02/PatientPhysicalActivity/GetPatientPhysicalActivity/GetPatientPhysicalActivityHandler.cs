using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientPhysicalActivity.GetPatientPhysicalActivity;

public record GetPatientPhysicalActivityV2Query(Guid PatientProfileId)
    : IQuery<GetPatientPhysicalActivityV2Result>;

public record GetPatientPhysicalActivityV2Result(
    IEnumerable<Models.PatientPhysicalActivity> PatientPhysicalActivities
);

public class GetPatientPhysicalActivityV2Handler(LifeStylesDbContext context)
    : IQueryHandler<GetPatientPhysicalActivityV2Query, GetPatientPhysicalActivityV2Result>
{
    public async Task<GetPatientPhysicalActivityV2Result> Handle(
        GetPatientPhysicalActivityV2Query request,
        CancellationToken cancellationToken)
    {
        var activities = await context.PatientPhysicalActivities
            .Where(ppa => ppa.PatientProfileId == request.PatientProfileId)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
            throw new LifeStylesNotFoundException("Hoạt động thể chất của người dùng", request.PatientProfileId);

        var activitiesDto = activities.Adapt<IEnumerable<Models.PatientPhysicalActivity>>();

        return new GetPatientPhysicalActivityV2Result(activitiesDto);
    }
}