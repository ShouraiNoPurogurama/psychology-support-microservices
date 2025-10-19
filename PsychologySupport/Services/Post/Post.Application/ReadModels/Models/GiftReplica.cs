using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post.Application.ReadModels.Models
{
    public class GiftReplica
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public Guid? MediaId { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset LastSyncedAt { get; set; }
    }
}
