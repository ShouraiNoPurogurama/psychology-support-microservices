namespace BuildingBlocks.Messaging.Events.Profile;

public record AggregatePatientProfileResponse(
    Guid Id,
    string FullName,
    string Gender,
    string? Allergies,
    string PersonalityTraits,
    string Address,
    string PhoneNumber,
    string Email,
    DateOnly BirthDate,
    
    //Job
    string JobTitle,
    string EducationLevel,
    string IndustryName); //Sau này có bệnh án thì thêm vào đây