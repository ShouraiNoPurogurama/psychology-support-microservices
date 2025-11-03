namespace BuildingBlocks.Messaging.Events.Queries.Profile;

public record AggregatePatientProfileResponse(
    string FullName,
    string Gender,
    string? Allergies,
    string PersonalityTraits,
    DateOnly BirthDate,
    
    //Job
    string JobTitle,
    string EducationLevel,
    string IndustryName); //Sau này có bệnh án thì thêm vào đây