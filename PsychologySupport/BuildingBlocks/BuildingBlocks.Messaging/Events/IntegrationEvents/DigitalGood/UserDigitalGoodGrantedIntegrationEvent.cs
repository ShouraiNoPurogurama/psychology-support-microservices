using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.DigitalGood
{
    public record UserDigitalGoodGrantedIntegrationEvent(
       Guid AliasId,                     
       DateTimeOffset ValidFrom, 
       DateTimeOffset ValidTo    
   ) : IntegrationEvent;
}
