using BuildingBlocks.DDD;
using MediatR;

namespace Profile.API.DoctorProfiles.Events
{
        public record DoctorProfileCreatedEvent(
        Guid UserId,
        string Gender,
        string Email,
        string PhoneNumber,
        DateTimeOffset? CreatedAt
    ) : INotification;
}
