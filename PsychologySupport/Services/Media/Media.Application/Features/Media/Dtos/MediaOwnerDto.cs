using Media.Domain.Enums;

namespace Media.Application.Features.Media.Dtos
{
    public record MediaOwnerDto
    {
        public MediaOwnerType MediaOwnerType { get; init; }
        public Guid MediaOwnerId { get; init; }
    }
}
