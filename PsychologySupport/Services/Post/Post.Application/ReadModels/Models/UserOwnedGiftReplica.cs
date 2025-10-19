using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
