namespace UserMemory.API.Shared.Dtos.Gemini.Embedding;

public class GeminiEmbeddingOptions
{
    public string Model { get; set; } = "models/gemini-embedding-001";
    public int OutputDimensionality { get; set; } = 3072;       
    public string? TaskType { get; set; } = "RETRIEVAL_QUERY";
    public bool Normalize { get; set; } = true;          
    
    public bool DedupEnabled { get; set; } = true;
    
    public double VectorSimThreshold { get; set; } = 0.92; //cosine ~ 0.92+
    
    public bool MergeTagsOnDuplicate { get; set; } = true;
    
    public bool TouchTimestampOnDuplicate { get; set; } = true; //cập nhật LastModified khi gặp trùng
}