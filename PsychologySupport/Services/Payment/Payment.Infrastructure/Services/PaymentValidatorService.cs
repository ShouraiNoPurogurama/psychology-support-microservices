using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Billing;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using BuildingBlocks.Messaging.Events.Queries.Scheduling;
using BuildingBlocks.Messaging.Events.Queries.Subscription;
using Mapster;
using MassTransit;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Infrastructure.Services;

public class PaymentValidatorService(
    IRequestClient<PatientProfileExistenceRequest> checkProfileClient,
    IRequestClient<ValidateSubscriptionRequest> checkSubscriptionClient,
    IRequestClient<ValidateBookingRequest> checkBookingClient)
    : IPaymentValidatorService
{
    public void ValidateVNPayMethod(PaymentMethodName paymentMethod)
    {
        if (paymentMethod != PaymentMethodName.VNPay)
        {
            throw new BadRequestException("Phương thức thanh toán không hợp lệ.");
        }
    }

    public void ValidatePayOSMethod(PaymentMethodName paymentMethod)
    {
        if (paymentMethod != PaymentMethodName.PayOS)
        {
            throw new BadRequestException("Phương thức thanh toán không hợp lệ.");
        }
    }

    public async Task ValidateSubscriptionRequestAsync(BuySubscriptionDto dto)
    {
        await ValidatePatientAsync(dto.PatientId);
        await ValidateSubscriptionAsync(dto);
    }

    public async Task ValidateSubscriptionRequestAsync(UpgradeSubscriptionDto dto)
    {
        await ValidatePatientAsync(dto.PatientId);
        await ValidateUpgradeSubscriptionAsync(dto);
    }

    public async Task ValidateBookingRequestAsync(BuyBookingDto dto)
    {
        await ValidatePatientAsync(dto.PatientId);
        await ValidateBookingAsync(dto);
    }

    public async Task ValidatePatientAsync(Guid patientId)
    {
        var patient =
            await checkProfileClient.GetResponse<PatientProfileExistenceResponse>(new PatientProfileExistenceRequest(patientId));

        if (!patient.Message.IsExist)
        {
            throw new BadRequestException("Không tìm thấy hồ sơ người dùng.");
        }
    }

    public async Task ValidateSubscriptionAsync(BuySubscriptionDto dto)
    {
        var validationResult =
            await checkSubscriptionClient.GetResponse<ValidateSubscriptionResponse>(dto.Adapt<ValidateSubscriptionRequest>() with
            {
                OldSubscriptionPrice = 0
            });

        if (!validationResult.Message.IsSuccess)
        {
            throw new BadRequestException("Dữ liệu không hợp lệ: " + string.Join(", ", validationResult.Message.Errors));
        }
    }

    public async Task ValidateUpgradeSubscriptionAsync(UpgradeSubscriptionDto dto)
    {
        var validationResult =
            await checkSubscriptionClient.GetResponse<ValidateSubscriptionResponse>(dto.Adapt<ValidateSubscriptionRequest>());

        if (!validationResult.Message.IsSuccess)
        {
            throw new BadRequestException("Dữ liệu không hợp lệ: " + string.Join(", ", validationResult.Message.Errors));
        }
    }

    public async Task ValidateBookingAsync(BuyBookingDto dto)
    {
        var validationResult =
            await checkBookingClient.GetResponse<ValidateBookingResponse>
            (dto.Adapt<ValidateBookingRequest>() with
                {
                    FinalPrice = dto.FinalPrice,
                    Date = dto.Date,
                    StartTime = dto.StartTime,
                }
            );

        if (!validationResult.Message.IsSuccess)
        {
            throw new BadRequestException("Dữ liệu không hợp lệ: " + string.Join(", ", validationResult.Message.Errors));
        }
    }

    public async Task ValidateOrderRequestAsync(OrderDto dto)
    {
        var validationResult =
            await checkSubscriptionClient.GetResponse<ValidateOrderResponse>(dto.Adapt<ValidateOrderRequest>());

        if (!validationResult.Message.IsSuccess)
        {
            throw new BadRequestException("Dữ liệu không hợp lệ: " + string.Join(", ", validationResult.Message.Errors));
        }
    }
}