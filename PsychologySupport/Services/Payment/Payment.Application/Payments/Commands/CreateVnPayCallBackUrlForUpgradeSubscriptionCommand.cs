using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Application.Payments.Commands;

public record CreateVnPayCallBackUrlForUpgradeSubscriptionCommand(UpgradeSubscriptionDto UpgradeSubscription)
    : ICommand<CreateVnPayCallBackUrlForUpgradeSubscriptionResult>;

public record CreateVnPayCallBackUrlForUpgradeSubscriptionResult(string Url);

public class CreateVnPayCallBackUrlForUpgradeSubscriptionCommandHandler(IVnPayService vnPayService, IPaymentDbContext dbContext)
    : ICommandHandler<CreateVnPayCallBackUrlForUpgradeSubscriptionCommand, CreateVnPayCallBackUrlForUpgradeSubscriptionResult>
{
    public async Task<CreateVnPayCallBackUrlForUpgradeSubscriptionResult> Handle(
        CreateVnPayCallBackUrlForUpgradeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UpgradeSubscription;

        var paymentMethod = await dbContext.PaymentMethods
                                .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken: cancellationToken)
                            ?? throw new NotFoundException(nameof(PaymentMethod), dto.PaymentMethod);

        var paymentId = Guid.NewGuid();

        var payment = Domain.Models.Payment.Create(
            paymentId,
            dto.PatientId,
            dto.PatientEmail,
            PaymentType.UpgradeSubscription,
            paymentMethod.Id,
            paymentMethod,
            dto.FinalPrice - dto.OldSubscriptionPrice,
            dto.SubscriptionId,
            null
        );

        dbContext.Payments.Add(payment);

        var vnPayUrl = await vnPayService.CreateVNPayUrlForUpgradeSubscriptionAsync(request.UpgradeSubscription, payment.Id);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateVnPayCallBackUrlForUpgradeSubscriptionResult(vnPayUrl);
    }
}