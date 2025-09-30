using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Queries.Media
{
    public record GetMediaUrlResponse
    {
        public Dictionary<Guid, string> Urls { get; init; } = new();
    }
}
