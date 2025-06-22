using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using System.Data.Common;

namespace Payment.Application.Payments.Commands;

public record CreatePayOSCallBackUrlForSubscriptionCommand(BuySubscriptionDto BuySubscription)
    : ICommand<CreatePayOSCallBackUrlForSubscriptionResult>;

public record CreatePayOSCallBackUrlForSubscriptionResult(long? PaymentCode, string Url);

public class CreatePayOSCallBackUrlForSubscriptionCommandHandler(
    IPayOSService payOSService,
    IPaymentDbContext dbContext)
    : ICommandHandler<CreatePayOSCallBackUrlForSubscriptionCommand, CreatePayOSCallBackUrlForSubscriptionResult>
{
    public async Task<CreatePayOSCallBackUrlForSubscriptionResult> Handle(
        CreatePayOSCallBackUrlForSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.BuySubscription;

        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentMethod), dto.PaymentMethod);

        var paymentId = Guid.NewGuid();

        var payment = Payment.Domain.Models.Payment.Create(
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

        // Create PaymentCode
        var db = (DbContext)dbContext;
        await db.Database.OpenConnectionAsync(cancellationToken);

        long nextCode;
        using (DbCommand cmd = db.Database.GetDbConnection().CreateCommand())
        {
            cmd.CommandText = "SELECT nextval('payment_code_seq')";
            cmd.CommandType = System.Data.CommandType.Text;

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            nextCode = Convert.ToInt64(result);
        }

        payment.PaymentCode = nextCode;

        var payOSUrl = await payOSService.CreatePayOSUrlForSubscriptionAsync(dto, payment.Id, nextCode);

        payment.PaymentUrl = payOSUrl;

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreatePayOSCallBackUrlForSubscriptionResult(nextCode,payOSUrl);
    }
}
