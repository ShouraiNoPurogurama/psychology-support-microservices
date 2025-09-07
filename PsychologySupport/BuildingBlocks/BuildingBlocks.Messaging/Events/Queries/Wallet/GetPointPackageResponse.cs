using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Queries.Wallet
{
    public record GetPointPackageResponse(
        string Code,
        string Name,
        decimal Price,
        string Currency,
        string? Description
    );
}
