namespace DigitalGoods.API.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // Tên của event
    public string Content { get; set; } = string.Empty; // Payload của event (dạng JSON)
    public DateTimeOffset OccuredOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }
}