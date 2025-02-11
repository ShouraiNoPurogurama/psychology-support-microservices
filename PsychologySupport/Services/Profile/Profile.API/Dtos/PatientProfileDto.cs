namespace Profile.API.Dtos
{
    public record PatientProfileDto(
        Guid Id,
        string? Allergies,
        string? MedicalHistory,
        string? Address,
        string? PhoneNumber,
        string? EmergencyContact
    );
}
