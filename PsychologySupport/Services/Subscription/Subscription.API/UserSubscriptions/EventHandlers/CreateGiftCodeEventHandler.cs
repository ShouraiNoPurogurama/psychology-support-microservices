using BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription;
using MassTransit;
using Promotion.Grpc;

namespace Subscription.API.UserSubscriptions.EventHandlers
{
    public class CreateGiftCodeEventHandler : IConsumer<CreateGiftCodeEvent>
    {
        private readonly PromotionService.PromotionServiceClient _promotionService;

        private readonly ILogger<CreateGiftCodeEventHandler> _logger;

        public CreateGiftCodeEventHandler(
            PromotionService.PromotionServiceClient promotionService,
            ILogger<CreateGiftCodeEventHandler> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateGiftCodeEvent> context)
        {
            var message = context.Message;

            if (!string.IsNullOrWhiteSpace(message.PromotionId))
            {
                // Get promotion details from gRPC
                var promotionResponse = await _promotionService.GetPromotionByIdAsync(
                    new GetPromotionByIdRequest { PromotionId = message.PromotionId }
                );

                var promotion = promotionResponse.Promotion;

                // Check Patient đã có GiftCode của Promotion này chưa
                var checkGiftCodeResponse = await _promotionService.GetGiftCodeByPatientPromotionIdAsync(
                    new GetGiftCodeByPatientPromotionIdRequest
                    {
                        PatientId = message.PatientId,
                        PromtotionId = message.PromotionId
                    }
                );

                if (checkGiftCodeResponse.GiftCodes.Count > 0)
                {
                    _logger.LogInformation(
                        "Patient {PatientId} already has a gift code for promotion {PromotionId}. Skipping creation.",
                        message.PatientId, message.PromotionId);
                    return;
                }

                // bo sung tra ve message cho FE

                var addGiftCodeRequest = new AddGiftCodesToPromotionRequest
                {
                    PromotionId = message.PromotionId,
                    CreateGiftCodeDto = new CreateGiftCodeDto
                    {
                        PatientId = message.PatientId,
                        MoneyValue = 0,
                        Title = promotion.PromotionType?.Name ,
                        Description = promotion.PromotionType?.Description,
                        Code = Guid.NewGuid().ToString("N")[..8].ToUpper()
                    }
                };

                await _promotionService.AddGiftCodesToPromotionAsync(addGiftCodeRequest);
            }
        }
    }
}