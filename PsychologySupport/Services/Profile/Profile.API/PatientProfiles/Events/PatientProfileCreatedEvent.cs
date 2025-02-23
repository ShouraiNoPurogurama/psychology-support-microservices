using BuildingBlocks.DDD;

namespace Profile.API.PatientProfiles.Events
{
    public record PatientProfileCreatedEvent(
        Guid UserId,
        string FullName,
        string Gender,
        string Email,
        string PhoneNumber,
        DateTimeOffset? CreatedAt
    ); 
}
