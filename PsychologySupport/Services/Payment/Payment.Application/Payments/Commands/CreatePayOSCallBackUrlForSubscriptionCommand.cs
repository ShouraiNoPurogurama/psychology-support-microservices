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

        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentMethod), dto.PaymentMethod);

        var db = (DbContext)dbContext;
        await db.Database.OpenConnectionAsync(cancellationToken);

        using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Generate new Payment ID and create Payment object
            var paymentId = Guid.NewGuid();

            // Get next PaymentCode from sequence
            long nextCode;
            using (DbCommand cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT nextval('payment_code_seq')";
                cmd.CommandType = System.Data.CommandType.Text;
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                nextCode = Convert.ToInt64(result);
            }

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

            payment.PaymentCode = nextCode;

            // Add payment to DB (initially without URL)
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Call PayOS to get payment URL
            var payOSUrl = await payOSService.CreatePayOSUrlForSubscriptionAsync(dto, paymentId, nextCode);

            // Update payment URL
            payment.PaymentUrl = payOSUrl;
            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new CreatePayOSCallBackUrlForSubscriptionResult(payOSUrl);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
