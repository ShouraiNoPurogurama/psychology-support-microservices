namespace Alias.API.Domains.Aliases.Dtos;

public record SuggestAliasesItemDto(string Label, string Token, DateTimeOffset ExpiredAt);