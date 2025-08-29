namespace ChatBox.API.Domains.Chatboxes.Dtos;

public class MessageRequestDto
{
    public string Content { get; set; }
    public Guid ReceiverId { get; set; }
}