using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using System.Data.Common;

namespace Payment.Application.Payments.Commands;

public record CreatePayOSCallBackUrlForBookingCommand(BuyBookingDto BuyBooking)
    : ICommand<CreatePayOSCallBackUrlForBookingResult>;

public record CreatePayOSCallBackUrlForBookingResult(string Url);

public class CreatePayOSCallBackUrlForBookingCommandHandler(IPayOSService payOSService, IPaymentDbContext dbContext)
    : ICommandHandler<CreatePayOSCallBackUrlForBookingCommand, CreatePayOSCallBackUrlForBookingResult>
{
    public async Task<CreatePayOSCallBackUrlForBookingResult> Handle(CreatePayOSCallBackUrlForBookingCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.BuyBooking;

        var paymentMethod = await dbContext.PaymentMethods
                                .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken: cancellationToken)
                            ?? throw new NotFoundException(nameof(PaymentMethod), dto.PaymentMethod);

        var paymentId = Guid.NewGuid();

        var payment = Domain.Models.Payment.Create(
            paymentId,
            dto.PatientId,
            dto.PatientEmail,
            PaymentType.Booking,
            paymentMethod.Id,
            paymentMethod,
            dto.FinalPrice,
            null,
            dto.BookingId
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

        var payOSUrl = await payOSService.CreatePayOSUrlForBookingAsync(request.BuyBooking, payment.Id, nextCode);

        payment.PaymentUrl = payOSUrl;

        dbContext.Payments.Add(payment);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreatePayOSCallBackUrlForBookingResult(payOSUrl);
    }
}