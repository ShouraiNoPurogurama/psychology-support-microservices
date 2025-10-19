namespace AIModeration.API.Shared.Dtos;

public record PostModerationResultDto
{
    public Guid PostId { get; init; }
    public string Status { get; init; } = string.Empty; // Approved, Rejected, Flagged
    public List<string> Reasons { get; init; } = new();
    public string PolicyVersion { get; init; } = string.Empty;
    public DateTimeOffset EvaluatedAt { get; init; }
}
