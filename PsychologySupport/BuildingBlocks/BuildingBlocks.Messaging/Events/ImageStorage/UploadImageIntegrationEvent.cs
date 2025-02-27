using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.ImageStorage
{
    public record UploadImageIntegrationEvent(
        Guid RequestId, 
        string Name,
        byte[] Data,
        string Extension,
        string OwnerType, 
        Guid OwnerId  
    );

}
