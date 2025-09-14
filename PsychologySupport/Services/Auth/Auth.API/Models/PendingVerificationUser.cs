using System.ComponentModel.DataAnnotations.Schema;
using BuildingBlocks.Data.Common;
using BuildingBlocks.DDD;

namespace Auth.API.Models;

public class PendingVerificationUser : Entity<Guid>, IHasCreationAudit
{
    [NotMapped] public string FullName { get; init; } = default!;
    public Guid UserId { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    //Chỉ 1 cột để lưu nguyên cục payload đã Protect()
    public byte[] PayloadProtected { get; set; } = default!;

    public User User { get; set; }
}