using BuildingBlocks.CQRS;
using Net.payOS.Types;

namespace Payment.Application.Payments.Commands;

public record CancelPaymentLinkCommand(
    long PaymentCode,
    string? CancellationReason = null) : ICommand<CancelPaymentLinkCommandResult>;

public record CancelPaymentLinkCommandResult(
    PaymentLinkInformation PaymentInfo,
    string Message = "Payment link cancelled successfully");

public class CancelPaymentLinkCommandHandler : ICommandHandler<CancelPaymentLinkCommand, CancelPaymentLinkCommandResult>
{
    private readonly IPayOSService _payOSService;

    public CancelPaymentLinkCommandHandler(IPayOSService payOSService)
    {
        _payOSService = payOSService;
    }

    public async Task<CancelPaymentLinkCommandResult> Handle(
        CancelPaymentLinkCommand request,
        CancellationToken cancellationToken)
    {
        var paymentInfo = await _payOSService.CancelPaymentLinkAsync(
            request.PaymentCode,
            request.CancellationReason);

        return new CancelPaymentLinkCommandResult(paymentInfo);
    }
}