using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientTherapeuticActivity.GetPatientTherapeuticActivity;

public record GetPatientTherapeuticActivityQuery(Guid PatientProfileId)
    : IQuery<GetPatientTherapeuticActivityResult>;

public record GetPatientTherapeuticActivityResult(IEnumerable<Models.PatientTherapeuticActivity> PatientTherapeuticActivities);

public class GetPatientTherapeuticActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetPatientTherapeuticActivityQuery, GetPatientTherapeuticActivityResult>
{
    public async Task<GetPatientTherapeuticActivityResult> Handle(GetPatientTherapeuticActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activities = await context.PatientTherapeuticActivities
            .Where(ppa => ppa.PatientProfileId == request.PatientProfileId)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
            throw new LifeStylesNotFoundException("Patient Therapeutic Activity", request.PatientProfileId);

        var activitiesDto = activities.Adapt<IEnumerable<Models.PatientTherapeuticActivity>>();

        return new GetPatientTherapeuticActivityResult(activitiesDto);
    }
}