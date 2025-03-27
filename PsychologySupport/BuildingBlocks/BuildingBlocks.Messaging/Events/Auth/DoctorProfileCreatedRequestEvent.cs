using BuildingBlocks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Auth
{
    public record DoctorProfileCreatedRequestEvent
    (
        string FullName,
        UserGender Gender,
        string Email,
        string PhoneNumber,
        string Password
    );
}
