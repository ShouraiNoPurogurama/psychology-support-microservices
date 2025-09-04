using BuildingBlocks.Messaging.Events.Queries.Translation;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Extensions;
using Profile.API.Models.Public;

namespace Profile.API.Domains.PatientProfiles.Features.GetJobByIndustryId
{
    public record GetJobByIndustryIdQuery(Guid IndustryId) : IQuery<GetJobByIndustryIdResult>;

    public record GetJobByIndustryIdResult(List<JobDto> Jobs);
    public class GetJobByIndustryIdHandler : IQueryHandler<GetJobByIndustryIdQuery, GetJobByIndustryIdResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

        public GetJobByIndustryIdHandler(ProfileDbContext context, IRequestClient<GetTranslatedDataRequest> translationClient)
        {
            _context = context;
            _translationClient = translationClient;
        }

        public async Task<GetJobByIndustryIdResult> Handle(GetJobByIndustryIdQuery request, CancellationToken cancellationToken)
        {
            var jobs = await _context.Jobs
                .Where(j => j.IndustryId == request.IndustryId)
                .ToListAsync(cancellationToken);

            var jobDtos = jobs.Adapt<List<JobDto>>();
            
            var translatedJobs = await jobDtos.TranslateEntitiesAsync(nameof(Job),
                _translationClient,
                j => j.Id.ToString(),
                cancellationToken,
                j => j.JobTitle,
                j => j.EducationLevel
                );

            return new GetJobByIndustryIdResult(translatedJobs);
        }
    }
}
