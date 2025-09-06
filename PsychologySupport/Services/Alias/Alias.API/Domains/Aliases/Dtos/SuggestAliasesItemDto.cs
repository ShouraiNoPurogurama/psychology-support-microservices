namespace Alias.API.Domains.Aliases.Dtos;

public record SuggestAliasesItemDto(string Label, string ReservationToken, DateTimeOffset ExpiredAt);