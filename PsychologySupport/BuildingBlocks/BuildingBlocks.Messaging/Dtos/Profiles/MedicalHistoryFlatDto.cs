namespace BuildingBlocks.Messaging.Dtos.Profiles;

public record MedicalHistoryFlatDto(    
    DateTimeOffset DiagnosedAt,
    List<string> SpecificMentalDisorders,
    List<string> PhysicalSymptoms);