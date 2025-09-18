using BuildingBlocks.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Domain.Events
{
    public record OrderPaymentDetailCompletedEvent
    (
        Guid SubjectRef,
        Guid OrderId,
        string PatientEmail,
        decimal FinalPrice
   ) : IDomainEvent;
}
