namespace ChatBox.API.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderUserId { get; set; }
    public Guid ReceiverUserId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public bool IsRead { get; set; }
}