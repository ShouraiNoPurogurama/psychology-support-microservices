using BuildingBlocks.Enums;
using MassTransit.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Queries.Billing
{
    public record GenerateOrderPaymentUrlRequest(
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentMethodName PaymentMethodName,
    Guid SubjectRef,
    string PointPackageCode
);

}
