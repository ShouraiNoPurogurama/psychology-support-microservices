namespace ChatBox.API.Models;

public class AIChatSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Name { get; set; } = "Đoạn chat mới";
    public bool? IsActive { get; set; } = true;
}