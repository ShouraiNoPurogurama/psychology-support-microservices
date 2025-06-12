using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientFoodActivity.GetPatientFoodActivity;

public record GetPatientFoodActivityQuery(Guid PatientProfileId)
    : IQuery<GetPatientFoodActivityResult>;

public record GetPatientFoodActivityResult(IEnumerable<Models.PatientFoodActivity> PatientFoodActivities);

public class GetPatientFoodActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetPatientFoodActivityQuery, GetPatientFoodActivityResult>
{
    public async Task<GetPatientFoodActivityResult> Handle(GetPatientFoodActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activities = await context.PatientFoodActivities
            .Where(ppa => ppa.PatientProfileId == request.PatientProfileId)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
            throw new LifeStylesNotFoundException("Patient Food Activity", request.PatientProfileId);

        var activitiesDto = activities.Adapt<IEnumerable<Models.PatientFoodActivity>>();

        return new GetPatientFoodActivityResult(activitiesDto);
    }
}