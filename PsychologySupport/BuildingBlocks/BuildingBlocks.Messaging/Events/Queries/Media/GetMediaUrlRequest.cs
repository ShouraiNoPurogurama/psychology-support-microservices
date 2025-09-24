using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Queries.Media
{
    public record GetMediaUrlRequest
    {
        public List<Guid> MediaIds { get; init; } = new();
    }
}
