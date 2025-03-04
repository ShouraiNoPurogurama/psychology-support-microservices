using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Profile;
using MassTransit;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using Payment.Domain.Enums;
using Payment.Domain.Models;

namespace Payment.Infrastructure.Services;

public class PaymentValidatorService : IPaymentValidatorService
{
    private readonly IRequestClient<PatientProfileExistenceRequest> _client;

    public PaymentValidatorService(IRequestClient<PatientProfileExistenceRequest> client)
    {
        _client = client;
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
        
        // var servicePackage = 
        
    }

    public Task ValidateBookingRequestAsync(BuySubscriptionDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task ValidatePatientAsync(Guid patientId)
    {
        var patient = await _client.GetResponse<PatientProfileExistenceResponse>(new PatientProfileExistenceRequest(patientId));
        
        if (!patient.Message.IsExist)
        {
            throw new BadRequestException("Patient not found");
        }
    }
}