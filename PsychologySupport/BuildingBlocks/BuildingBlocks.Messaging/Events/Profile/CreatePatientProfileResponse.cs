using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Profile
{
    public record CreatePatientProfileResponse(Guid PatientId, bool Success, string? Message = null);

}
