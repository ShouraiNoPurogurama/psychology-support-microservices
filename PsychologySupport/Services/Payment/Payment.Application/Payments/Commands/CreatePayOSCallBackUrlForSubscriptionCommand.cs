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

public record CreatePayOSCallBackUrlForSubscriptionResult(string Url);

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
        var db = (DbContext)dbContext;

        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentMethod), dto.PaymentMethod);

        await db.Database.OpenConnectionAsync(cancellationToken);

        long nextCode;
        Guid paymentId = Guid.NewGuid();

        using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            using (DbCommand cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT nextval('payment_code_seq')";
                cmd.CommandType = System.Data.CommandType.Text;
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                nextCode = Convert.ToInt64(result);
            }

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

            payment.PaymentCode = nextCode;
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        var payOSUrl = await payOSService.CreatePayOSUrlForSubscriptionAsync(dto, paymentId, nextCode);

        var createdPayment = await dbContext.Payments.FirstAsync(p => p.Id == paymentId, cancellationToken);
        createdPayment.PaymentUrl = payOSUrl;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreatePayOSCallBackUrlForSubscriptionResult(payOSUrl);
    }
}
