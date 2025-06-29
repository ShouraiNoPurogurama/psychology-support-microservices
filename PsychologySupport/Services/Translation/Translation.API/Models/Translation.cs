namespace Translation.API.Models;

public class Translation
{
    public Guid Id { get; set; }
    public string TextKey { get; set; } = default!;
    public string Lang { get; set; } = default!;
    public string TranslatedValue { get; set; } = default!;
    
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public bool IsStable { get; set; } = false;
}