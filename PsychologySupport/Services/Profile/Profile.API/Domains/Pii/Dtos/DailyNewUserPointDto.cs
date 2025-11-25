namespace Profile.API.Domains.Pii.Dtos;

public record DailyNewUserPointDto(
    DateOnly Date,
    int NewUserCount);