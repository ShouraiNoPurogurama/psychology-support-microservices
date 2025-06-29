using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Utils;
using Subscription.API.Data;
using Subscription.API.Exceptions;
using Subscription.API.ServicePackages.Dtos;
using MassTransit;
using Subscription.API.ServicePackages.Models;

namespace Subscription.API.ServicePackages.Features.GetServicePackage;

public record GetServicePackageQuery(Guid Id) : IQuery<GetServicePackageResult>;

public record GetServicePackageResult(ServicePackageDto ServicePackage);

public class GetServicePackageHandler : IQueryHandler<GetServicePackageQuery, GetServicePackageResult>
{
    private readonly SubscriptionDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetServicePackageHandler(
        SubscriptionDbContext context,
        IRequestClient<GetTranslatedDataRequest> translationClient)
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetServicePackageResult> Handle(GetServicePackageQuery query, CancellationToken cancellationToken)
    {
        var servicePackage = await _context.ServicePackages.FindAsync(new object[] { query.Id }, cancellationToken)
                             ?? throw new SubscriptionNotFoundException("Service Package", query.Id);

        var dto = new ServicePackageDto(
            servicePackage.Id,
            servicePackage.Name,
            servicePackage.Description,
            servicePackage.Price,
            servicePackage.DurationDays,
            servicePackage.ImageId,
            servicePackage.IsActive
        );

        // Build translation request
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities([dto], nameof(ServicePackage), x => x.Name, x => x.Description)
            .Build();

        var response = await _translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi), cancellationToken);

        var translations = response.Message.Translations;

        var translatedDto = dto with
        {
            Name = translations.GetTranslatedValue(dto, x => x.Name, nameof(ServicePackage)),
            Description = translations.GetTranslatedValue(dto, x => x.Description, nameof(ServicePackage))
        };

        return new GetServicePackageResult(translatedDto);
    }
}
