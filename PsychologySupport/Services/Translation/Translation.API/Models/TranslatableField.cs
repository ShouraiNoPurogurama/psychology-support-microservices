namespace Translation.API.Models;

public class TranslatableField
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = default!;
    public string FieldName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? CreatedAt { get; set; }
}