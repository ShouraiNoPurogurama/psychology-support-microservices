using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Auth
{
    public record DoctorProfileCreatedResponseEvent(Guid UserId, bool Success);
}
