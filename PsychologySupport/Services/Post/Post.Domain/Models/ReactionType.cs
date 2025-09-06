// namespace Post.Domain.Models;
//
// public sealed class ReactionType : Entity<Guid>
// {
//     public Guid? PackId { get; set; }                //NULL = built-in/free
//     public string DisplayName { get; set; } = null!;
//     public Guid? MediaId { get; set; }               //icon designer (Media Service)
//     public int SortOrder { get; set; }
//     public bool IsActive { get; set; } = true;
//     public string? Topic { get; set; }               //optional: group UI
//
//     public ReactionPack? Pack { get; set; }
// }