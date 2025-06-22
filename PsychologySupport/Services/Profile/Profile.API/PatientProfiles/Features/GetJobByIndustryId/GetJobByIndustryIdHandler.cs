using Mapster;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetJobByIndustryId
{
    public record GetJobByIndustryIdQuery(Guid IndustryId) : IQuery<GetJobByIndustryIdResult>;

    public record GetJobByIndustryIdResult(List<JobDto> Jobs);
    public class GetJobByIndustryIdHandler : IQueryHandler<GetJobByIndustryIdQuery, GetJobByIndustryIdResult>
    {
        private readonly ProfileDbContext _context;

        public GetJobByIndustryIdHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<GetJobByIndustryIdResult> Handle(GetJobByIndustryIdQuery request, CancellationToken cancellationToken)
        {
            var jobs = await _context.Jobs
                .Where(j => j.IndustryId == request.IndustryId)
                .ToListAsync(cancellationToken);

            var jobDtos = jobs.Adapt<List<JobDto>>();

            return new GetJobByIndustryIdResult(jobDtos);
        }
    }
}
