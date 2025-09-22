using BuildingBlocks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application.Payments.Dtos
{
    public record OrderDto
    (
        Guid OrderId,
        long OrderCode,
        string ProductCode,// PointPackageCode
        Guid SubjectRef,
        string? PromoCode,
        Guid? GiftId,
        string PointPackageName,
        string PointPackageDescription,
        int PointAmount,
        PaymentMethodName PaymentMethodName,
        PaymentType PaymentType,
        decimal FinalPrice
   ) : BasePaymentDto(FinalPrice, SubjectRef, PaymentMethodName);

}
