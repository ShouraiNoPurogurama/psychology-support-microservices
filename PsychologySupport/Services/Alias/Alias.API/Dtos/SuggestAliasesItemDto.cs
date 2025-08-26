namespace Alias.API.Dtos;

public record SuggestAliasesItemDto(string Label, string Token, DateTimeOffset ExpiredAt);