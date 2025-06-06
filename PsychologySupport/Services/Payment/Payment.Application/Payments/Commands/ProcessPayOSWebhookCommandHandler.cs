using System.Transactions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Payment.Application.Data;

namespace Payment.Application.Payments.Commands;

public record ProcessPayOSWebhookCommand(WebhookData WebhookData) : ICommand<ProcessPayOSWebhookResult>;
public record ProcessPayOSWebhookResult(bool Success);

public class ProcessPayOSWebhookCommandHandler : ICommandHandler<ProcessPayOSWebhookCommand, ProcessPayOSWebhookResult>
{
    private readonly IPaymentDbContext _dbContext;

    public ProcessPayOSWebhookCommandHandler(IPaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProcessPayOSWebhookResult> Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhookData = request.WebhookData;
        var orderCode = LongToGuid(webhookData.orderCode);

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == orderCode, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Models.Payment), orderCode);

        var desc = webhookData.desc?.ToUpperInvariant() ?? string.Empty;
        var amount = webhookData.amount / 100m; 

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            if (desc.Contains("THÀNH CÔNG") || desc.Contains("SUCCESS"))
            {

                payment.AddPaymentDetail(
                    PaymentDetail.Of(amount, webhookData.reference).MarkAsSuccess()
                );
                payment.MarkAsCompleted("unknown@example.com");
            }
            else if (desc.Contains("HUỶ") || desc.Contains("CANCELLED") || desc.Contains("THẤT BẠI") || desc.Contains("FAILED"))
            {

                payment.AddFailedPaymentDetail(
                    PaymentDetail.Of(amount, webhookData.reference),
                    "unknown@example.com",
                    null, 
                    null 
                );
            }
            else
            {
                throw new BadRequestException("Invalid payment status in webhook description");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            scope.Complete();
        }

        return new ProcessPayOSWebhookResult(true);
    }

    private static Guid LongToGuid(long value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}