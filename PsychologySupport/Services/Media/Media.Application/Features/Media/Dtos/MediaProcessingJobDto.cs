using Media.Domain.Enums;

namespace Media.Application.Features.Media.Dtos
{
    public record MediaProcessingJobDto(
       Guid JobId,
       JobType JobType,
       ProcessStatus Status
   );
}
