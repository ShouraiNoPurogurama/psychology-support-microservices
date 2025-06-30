using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Promotion.Grpc;

namespace Subscription.API.UserSubscriptions.EventHandlers
{
    public class CreateGiftCodeEventHandler : IConsumer<CreateGiftCodeEvent>
    {
        private readonly PromotionService.PromotionServiceClient _promotionService;

        public CreateGiftCodeEventHandler(PromotionService.PromotionServiceClient promotionService)
        {
            _promotionService = promotionService;
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

                // check xem patient đã có GiftCode của Promotion này chưa
                // bổ sung

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