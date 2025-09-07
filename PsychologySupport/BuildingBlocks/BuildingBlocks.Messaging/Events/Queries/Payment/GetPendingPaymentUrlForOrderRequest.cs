using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Queries.Payment
{
    public record GetPendingPaymentUrlForOrderRequest(Guid OrderId);
    
}
