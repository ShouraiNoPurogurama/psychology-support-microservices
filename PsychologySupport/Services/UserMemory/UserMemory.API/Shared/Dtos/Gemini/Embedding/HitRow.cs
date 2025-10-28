namespace UserMemory.API.Shared.Dtos.Gemini.Embedding;

public sealed class HitRow
{
    public Guid id { get; set; }
    public double similarity_score { get; set; }
}