using System.Text.Json;
using BuildingBlocks.DDD;
using UserMemory.API.Shared.Enums;

namespace UserMemory.API.Models;

public class Reward : AuditableEntity<Guid>
{
    public Guid AliasId { get; set; }

    public Guid SessionId { get; set; }

    public int PointsCost { get; set; }

    public RewardStatus Status { get; set; } = RewardStatus.Pending;

    public string? StickerUrl { get; set; }

    public string? PromptBase { get; set; }

    public string? PromptFiller { get; set; }

    public JsonDocument? Meta { get; set; }
}