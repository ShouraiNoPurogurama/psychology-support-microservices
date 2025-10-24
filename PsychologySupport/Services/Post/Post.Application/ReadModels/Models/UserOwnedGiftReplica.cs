namespace Post.Application.ReadModels.Models
{
    public class UserOwnedGiftReplica
    {
        public Guid AliasId { get; set; }
        public Guid GiftId { get; set; }

        public DateTimeOffset LastSyncedAt { get; set; }

        public DateTimeOffset ValidFrom { get; set; }

        public DateTimeOffset ValidTo { get; set; }
    }
}
