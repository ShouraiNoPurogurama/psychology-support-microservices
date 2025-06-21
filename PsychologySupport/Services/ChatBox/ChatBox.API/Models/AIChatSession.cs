using BuildingBlocks.DDD;

namespace ChatBox.API.Models;

public class AIChatSession : DomainEventContainer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Name { get; set; } = "Đoạn chat mới";
    public bool? IsActive { get; set; } = true;

    // Thêm cho mục đích tóm tắt
    public string? Summarization { get; set; }  
    public DateTime? LastSummarizedAt { get; set; } 
    public int? LastSummarizedIndex { get; set; } 
}

