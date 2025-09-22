using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application.Payments.Commands
{
    public record CreatePayOSCallBackUrlForOrderCommand(OrderDto OrderPayment)
    : ICommand<CreatePayOSCallBackUrlForOrderResult>;

    public record CreatePayOSCallBackUrlForOrderResult(long? PaymentCode, string Url);

    public class CreatePayOSCallBackUrlForOrderCommandHandler(
        IPayOSService payOSService,
        IPaymentDbContext dbContext)
        : ICommandHandler<CreatePayOSCallBackUrlForOrderCommand, CreatePayOSCallBackUrlForOrderResult>
    {
        public async Task<CreatePayOSCallBackUrlForOrderResult> Handle(
            CreatePayOSCallBackUrlForOrderCommand request,
            CancellationToken cancellationToken)
        {
            var dto = request.OrderPayment;

            var paymentMethod = await dbContext.PaymentMethods
                .FirstOrDefaultAsync(p => p.Name == dto.PaymentMethod, cancellationToken)
                ?? throw new NotFoundException(nameof(PaymentMethod), dto.PaymentMethod);

            var paymentId = Guid.NewGuid();

            var payment = Payment.Domain.Models.Payment.Create(
                paymentId,
                dto.SubjectRef,
                "none@gmail.com",
                PaymentType.BuyPointPackage,
                paymentMethod.Id,
                paymentMethod,
                dto.FinalPrice,
                dto.OrderId,
                null
            );

            // Generate next PaymentCode
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

            var payOSUrl = await payOSService.CreatePayOSUrlForOrderAsync(dto, payment.Id, dto.OrderCode);

            payment.PaymentUrl = payOSUrl;

            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new CreatePayOSCallBackUrlForOrderResult(nextCode, payOSUrl);
        }
    }
}
