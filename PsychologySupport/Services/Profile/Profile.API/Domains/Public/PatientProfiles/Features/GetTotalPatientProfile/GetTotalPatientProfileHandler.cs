using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetTotalPatientProfile
{
    public record GetTotalPatientProfileQuery(DateOnly StartDate, DateOnly EndDate, UserGender? Gender = null) : IRequest<int>;

    public class GetTotalPatientProfileHandler : IRequestHandler<GetTotalPatientProfileQuery, int>
    {
        private readonly ProfileDbContext _context;

        public GetTotalPatientProfileHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(GetTotalPatientProfileQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PatientProfile> query = _context.PatientProfiles;

            query = query.Where(p => p.CreatedAt.HasValue &&
                DateOnly.FromDateTime(p.CreatedAt.Value.UtcDateTime) >= request.StartDate &&
                DateOnly.FromDateTime(p.CreatedAt.Value.UtcDateTime) <= request.EndDate);

      
            //TODO quay lại sửa sau
            
            // if (request.Gender.HasValue)
            // {
            //     query = query.Where(p => p.Gender == request.Gender.Value);
            // }

            return await query.CountAsync(cancellationToken);
        }
    }
}
