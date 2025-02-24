using BuildingBlocks.Data.Enums;
using BuildingBlocks.DDD;

namespace Profile.API.DoctorProfiles.Events
{
    public record DoctorProfileCreatedEvent(
        Guid UserId,
        UserGender Gender,
        string Email,
        string PhoneNumber,
        DateTimeOffset? CreatedAt
    ) : IDomainEvent;
}
