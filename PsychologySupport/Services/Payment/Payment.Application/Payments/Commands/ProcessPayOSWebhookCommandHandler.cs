using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Payment.Application.Data;
using Payment.Domain.Enums;

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

        var desc = webhookData.desc?.ToUpperInvariant() ?? "";

        if (desc != null)
        {
            var descUpper = desc.ToUpperInvariant();

            if (descUpper.Contains("THÀNH CÔNG") || descUpper.Contains("SUCCESS"))
            {
                payment.Status = PaymentStatus.Completed;
            }
            else if (descUpper.Contains("HUỶ") || descUpper.Contains("CANCELLED"))
            {
                payment.Status = PaymentStatus.Cancelled;
            }
            else if (descUpper.Contains("THẤT BẠI") || descUpper.Contains("FAILED"))
            {
                payment.Status = PaymentStatus.Failed;
            }
            else
            {
                payment.Status = PaymentStatus.None;
            }
        }
        else
        {
            payment.Status = PaymentStatus.None;
        }


        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProcessPayOSWebhookResult(true);
    }

    private static Guid LongToGuid(long value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

}
