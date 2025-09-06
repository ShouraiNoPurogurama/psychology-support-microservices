// namespace Post.Domain.Models;
//
// public sealed class ReactionPackEntitlement : Entity<Guid>
// {
//     public Guid AliasId { get; set; }                //alias sở hữu
//     public Guid PackId { get; set; }                 //pack được mở khóa
//     public DateTimeOffset GrantedAt { get; set; }
//     public DateTimeOffset? ExpiresAt { get; set; }
//     public string Source { get; set; } = "purchase"; //purchase | promo | gift ...
//
//     // Điều hướng (optional)
//     public ReactionPack? Pack { get; set; }
// }