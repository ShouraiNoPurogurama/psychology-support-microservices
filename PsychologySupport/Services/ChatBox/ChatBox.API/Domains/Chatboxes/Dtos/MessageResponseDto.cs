namespace ChatBox.API.Domains.Chatboxes.Dtos;


public class MessageResponseDto
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    
}