namespace Profile.API.Domains.Pii.Dtos;

public record SimplePiiProfileDto(string FullName, DateOnly BirthDate, string Gender);