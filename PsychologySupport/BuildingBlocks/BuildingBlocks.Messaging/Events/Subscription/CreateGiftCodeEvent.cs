using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Subscription
{
    public class CreateGiftCodeEvent
    {
        public string PatientId { get; set; } = default!;
        public string PromotionId { get; set; } = default!;
    }

}
