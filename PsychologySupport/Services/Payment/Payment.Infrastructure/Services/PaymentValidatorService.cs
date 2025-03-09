using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Profile;
using BuildingBlocks.Messaging.Events.Subscription;
using Mapster;
using MassTransit;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Infrastructure.Services;

public class PaymentValidatorService : IPaymentValidatorService
{
    private readonly IRequestClient<PatientProfileExistenceRequest> _checkProfileClient;
    private readonly IRequestClient<ValidateSubscriptionRequest> _checkSubscriptionClient;

    public PaymentValidatorService(IRequestClient<PatientProfileExistenceRequest> checkProfileClient, IRequestClient<ValidateSubscriptionRequest> checkSubscriptionClient)
    {
        _checkProfileClient = checkProfileClient;
        _checkSubscriptionClient = checkSubscriptionClient;
    }

    public void ValidateVNPayMethod(PaymentMethodName paymentMethod)
    {
        if (paymentMethod != PaymentMethodName.VNPay)
        {
            throw new BadRequestException("Invalid payment method");
        }
    }

    public async Task ValidateSubscriptionRequestAsync(BuySubscriptionDto dto)
    {
        ValidateVNPayMethod(dto.PaymentMethod);
        await ValidatePatientAsync(dto.PatientId);
        await ValidateSubscriptionAsync(dto);

    }

    public Task ValidateBookingRequestAsync(BuySubscriptionDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task ValidatePatientAsync(Guid patientId)
    {
        var patient =
            await _checkProfileClient.GetResponse<PatientProfileExistenceResponse>(new PatientProfileExistenceRequest(patientId));

        if (!patient.Message.IsExist)
        {
            throw new BadRequestException("Patient not found");
        }
    }

    public async Task ValidateSubscriptionAsync(BuySubscriptionDto dto)
    {
        var validationResult =
            await _checkSubscriptionClient.GetResponse<ValidateSubscriptionResponse>(dto.Adapt<ValidateSubscriptionRequest>());

        if (!validationResult.Message.IsSuccess)
        {
            throw new BadRequestException(string.Join(", ", validationResult.Message.Errors));
        }
    }
}