namespace ChatBox.API.Models;

public class AIMessage
{
    public Guid Id { get; set; }
    
    public Guid? SenderUserId { get; set; }
    
    public bool SenderIsEmo { get; set; }
    
    public Guid SessionId { get; set; }

    public int BlockNumber { get; set; } = 0;
    
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    
    public bool IsRead { get; set; } = false;

    public AIChatSession Session { get; set; }
}