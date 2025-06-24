namespace BuildingBlocks.Messaging.Dtos.Profiles;

public record MedicalRecordFlatDto(DateTimeOffset CreatedAt,
    List<string> SpecificMentalDisorders,
    List<MedicalHistoryFlatDto> MedicalHistories);