namespace Alias.API.Aliases.Dtos;

public record SuggestAliasesItemDto(string Label, string ReservationToken, DateTimeOffset ExpiredAt);