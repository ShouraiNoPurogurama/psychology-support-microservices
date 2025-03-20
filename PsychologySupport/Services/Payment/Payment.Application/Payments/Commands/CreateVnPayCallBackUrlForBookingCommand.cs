using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Application.Payments.Commands;

public record CreateVnPayCallBackUrlForBookingCommand(BuyBookingDto BuyBooking)
    : ICommand<CreateVnPayCallBackUrlForBookingCommandResult>;

public record CreateVnPayCallBackUrlForBookingCommandResult(string Url);

public class CreateVnPayCallBackUrlForBookingCommandHandler(IVnPayService vnPayService, IPaymentDbContext dbContext)
    : ICommandHandler<CreateVnPayCallBackUrlForBookingCommand, CreateVnPayCallBackUrlForBookingCommandResult>
{
    public async Task<CreateVnPayCallBackUrlForBookingCommandResult> Handle(CreateVnPayCallBackUrlForBookingCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.BuyBooking;

        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken: cancellationToken);

        var paymentId = Guid.NewGuid();
        
        var payment = Domain.Models.Payment.Create(
            paymentId,
            dto.PatientId,
            null,
            dto.PaymentType,
            paymentMethod.Id,
            paymentMethod,
            dto.FinalPrice,
            null,
            dto.BookingId
        );

        dbContext.Payments.Add(payment);

        await dbContext.SaveChangesAsync(cancellationToken);

        var vnPayUrl = await vnPayService.CreateVNPayUrlForBookingAsync(request.BuyBooking, payment.Id);

        return new CreateVnPayCallBackUrlForBookingCommandResult(vnPayUrl);
    }
}