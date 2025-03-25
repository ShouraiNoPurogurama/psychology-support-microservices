using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Profile
{
    public record CreatePatientProfileRequest(
       Guid UserId,
       string FullName,
       UserGender Gender,
       string? Allergies,
       PersonalityTrait PersonalityTraits,
       ContactInfo ContactInfo
   );
}
