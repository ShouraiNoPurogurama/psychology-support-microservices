using BuildingBlocks.Messaging.Events.Queries.Media;
using MassTransit;
using Media.Domain.Models;
using Media.Domain.Repositories;

namespace Media.Application.Features.Media.EventHandlers
{
    public class GetMediaUrlHandler : IConsumer<GetMediaUrlRequest>
    {
        private readonly IMediaAssetRepository _repository;

        public GetMediaUrlHandler(IMediaAssetRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetMediaUrlRequest> context)
        {
            var request = context.Message;

            var assets = await _repository.GetByIdsAsync(request.MediaIds.Distinct());

            var urls = new Dictionary<Guid, string>();

            foreach (var asset in assets)
            {
                var variant = asset.GetOriginalVariant() ?? asset.GetThumbnailVariant();

                if (variant is not null && variant.HasCdnUrl)
                {
                    urls[asset.Id] = variant.CdnUrl!;
                }
            }

            var response = new GetMediaUrlResponse
            {
                Urls = urls
            };

            await context.RespondAsync(response);
        }

    }
}
