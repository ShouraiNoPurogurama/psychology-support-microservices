using BuildingBlocks.CQRS;
using Payment.Application.Payment.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Application.Payment.Commands;

public record CreateSubscriptionVnPayCommand(BuySubscriptionDto BuySubscription) : ICommand<CreateSubscriptionVnPayResult>;

public record CreateSubscriptionVnPayResult(string Url);

public class CreateSubscriptionVnPayCommandHandler : ICommandHandler<CreateSubscriptionVnPayCommand, CreateSubscriptionVnPayResult>
{
    private readonly IVNPayService _vnPayService;

    public CreateSubscriptionVnPayCommandHandler(IVNPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    public async Task<CreateSubscriptionVnPayResult> Handle(CreateSubscriptionVnPayCommand request, CancellationToken cancellationToken)
    {
        var vnPayUrl = await _vnPayService.CreateVNPayUrlForSubscriptionAsync(request.BuySubscription);
        
        return new CreateSubscriptionVnPayResult(vnPayUrl);
    }
}