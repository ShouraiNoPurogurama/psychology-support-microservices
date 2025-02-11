using BuildingBlocks.CQRS;
using Profile.API.Data;
using Profile.API.Exceptions;
using Profile.API.Models;

namespace Profile.API.Features.PatientProfiles.GetPatientProfile
{
    public record GetPatientProfileQuery(Guid Id) : IQuery<GetPatientProfileResult>;
    public record GetPatientProfileResult(PatientProfile PatientProfile);
    public class GetPatientProfileHandler : IQueryHandler<GetPatientProfileQuery, GetPatientProfileResult>
    {
        private readonly ProfileDbContext _context;

        public GetPatientProfileHandler(ProfileDbContext context)
        {
            _context = context;
        }
        public async Task<GetPatientProfileResult> Handle(GetPatientProfileQuery query, CancellationToken cancellationToken)
        {
            var patientProfile = await _context.PatientProfiles.FindAsync(query.Id)
                            ?? throw new ProfileNotFoundException(query.Id.ToString());

            return new GetPatientProfileResult(patientProfile);
        }
    }
}
