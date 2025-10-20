using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Utils;
using Subscription.API.Data;
using Subscription.API.Exceptions;
using Subscription.API.ServicePackages.Dtos;
using Subscription.API.ServicePackages.Models;
using Translation.API.Protos; 

namespace Subscription.API.ServicePackages.Features02.GetServicePackage;

public record GetServicePackageQuery(Guid Id) : IQuery<GetServicePackageResult>;

public record GetServicePackageResult(ServicePackageDto ServicePackage);

public class GetServicePackageHandler(
    SubscriptionDbContext context,
    TranslationService.TranslationServiceClient translationClient) 
    : IQueryHandler<GetServicePackageQuery, GetServicePackageResult>
{
    private readonly SubscriptionDbContext _context = context;
    private readonly TranslationService.TranslationServiceClient _translationClient = translationClient;

    public async Task<GetServicePackageResult> Handle(GetServicePackageQuery query, CancellationToken cancellationToken)
    {
        var servicePackage = await _context.ServicePackages.FindAsync(new object[] { query.Id }, cancellationToken)
                             ?? throw new SubscriptionNotFoundException("Service Package", query.Id);

        var dto = new ServicePackageDto(
            servicePackage.Id,
            servicePackage.Name,
            servicePackage.Description,
            servicePackage.Price,
            servicePackage.OriginalPrice,
            servicePackage.DiscountLabel,
            servicePackage.DurationDays,
            servicePackage.ImageId,
            servicePackage.IsActive
        );

        // Build translation request
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities([dto], nameof(ServicePackage), x => x.Name, x => x.Description)
            .Build();

        // Chuyển đổi translationDict thành TranslateDataRequest
        var translateRequest = new TranslateDataRequest
        {
            Originals = { translationDict },
            TargetLanguage = SupportedLang.vi.ToString()
        };

        // Gọi gRPC TranslateData
        var response = await _translationClient.TranslateDataAsync(translateRequest, cancellationToken: cancellationToken);

        var translations = response.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var translatedDto = dto with
        {
            Name = translations.GetTranslatedValue(dto, x => x.Name, nameof(ServicePackage)),
            Description = translations.GetTranslatedValue(dto, x => x.Description, nameof(ServicePackage))
        };

        return new GetServicePackageResult(translatedDto);
    }
}