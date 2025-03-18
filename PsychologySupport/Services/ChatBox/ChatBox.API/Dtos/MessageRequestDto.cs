namespace ChatBox.API.Dtos;

public class MessageRequestDto
{
    public string Content { get; set; }
    public Guid ReceiverId { get; set; }
}