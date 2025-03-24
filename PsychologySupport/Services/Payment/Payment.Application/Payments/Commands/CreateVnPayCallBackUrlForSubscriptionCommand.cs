using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Application.Payments.Commands;

public record CreateVnPayCallBackUrlForSubscriptionCommand(BuySubscriptionDto BuySubscription) : ICommand<CreateVnPayCallBackUrlForSubscriptionResult>;

public record CreateVnPayCallBackUrlForSubscriptionResult(string Url);

public class CreateVnPayCallBackUrlForSubscriptionCommandHandler(IVnPayService vnPayService, IPaymentDbContext dbContext)
    : ICommandHandler<CreateVnPayCallBackUrlForSubscriptionCommand, CreateVnPayCallBackUrlForSubscriptionResult>
{
    public async Task<CreateVnPayCallBackUrlForSubscriptionResult> Handle(CreateVnPayCallBackUrlForSubscriptionCommand request, CancellationToken cancellationToken)
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
            dto.FinalPrice - dto.OldSubscriptionPrice,
            dto.SubscriptionId,
            null
        );
        
        dbContext.Payments.Add(payment);

        var vnPayUrl = await vnPayService.CreateVNPayUrlForSubscriptionAsync(request.BuySubscription, payment.Id);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return new CreateVnPayCallBackUrlForSubscriptionResult(vnPayUrl);
    }
}