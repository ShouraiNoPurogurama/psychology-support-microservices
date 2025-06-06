using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Application.Payments.Commands;

public record CreatePayOSCallBackUrlForSubscriptionCommand(BuySubscriptionDto BuySubscription)
    : ICommand<CreatePayOSCallBackUrlForSubscriptionResult>;

public record CreatePayOSCallBackUrlForSubscriptionResult(string Url);

public class CreatePayOSCallBackUrlForSubscriptionCommandHandler(IPayOSService payOSService, IPaymentDbContext dbContext)
    : ICommandHandler<CreatePayOSCallBackUrlForSubscriptionCommand, CreatePayOSCallBackUrlForSubscriptionResult>
{
    public async Task<CreatePayOSCallBackUrlForSubscriptionResult> Handle(CreatePayOSCallBackUrlForSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.BuySubscription;

        var paymentMethod = await dbContext.PaymentMethods
                                .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken: cancellationToken)
                            ?? throw new NotFoundException(nameof(PaymentMethod), dto.PaymentMethod);

        var paymentId = Guid.NewGuid();

        var payment = Domain.Models.Payment.Create(
            paymentId,
            dto.PatientId,
            dto.PatientEmail,
            PaymentType.BuySubscription,
            paymentMethod.Id,
            paymentMethod,
            dto.FinalPrice,
            dto.SubscriptionId,
            null
        );

        var payOSUrl = await payOSService.CreatePayOSUrlForSubscriptionAsync(request.BuySubscription, payment.Id);

        payment.PaymentUrl = payOSUrl;

        dbContext.Payments.Add(payment);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreatePayOSCallBackUrlForSubscriptionResult(payOSUrl);
    }
}