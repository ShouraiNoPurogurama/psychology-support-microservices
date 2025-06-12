using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Scheduling
{
    public record BookingGetPromoAndGiftResponseEvent(
         string? PromoCode,
         Guid? GiftId
     );
}
