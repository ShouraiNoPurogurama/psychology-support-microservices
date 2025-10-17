namespace AIModeration.API.Shared.Dtos;

public record AliasLabelModerationResultDto
{
    public bool IsValid { get; init; }
    public List<string> Reasons { get; init; } = new();
    public string PolicyVersion { get; init; } = string.Empty;
    public DateTimeOffset EvaluatedAt { get; init; }
}
