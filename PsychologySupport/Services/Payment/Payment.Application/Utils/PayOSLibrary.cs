using Net.payOS;
using Net.payOS.Types;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Payment.Application.Utils;

public class PayOSLibrary
{
    private readonly PayOS _client;
    private readonly string _checksumKey;

    public PayOSLibrary(PayOS client, IConfiguration configuration)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new ArgumentException("ChecksumKey not found in configuration");
    }

    public async Task<string> CreatePaymentLinkAsync(
        long orderCode,
        int amount,
        string description,
        string returnUrl,
        string cancelUrl)
    {
        var items = new List<ItemData>();

        var payload = new PaymentData(
            orderCode,
            amount,
            description,
            items,
            returnUrl,
            cancelUrl
        );

        var response = await _client.createPaymentLink(payload);
        return response.checkoutUrl ?? throw new Exception("Failed to create PayOS payment URL");
    }

    public async Task<WebhookData> VerifyWebhookDataAsync(string webhookJson)
    {
        try
        {
            // Deserialize the webhook JSON into WebhookType
            var webhookType = JsonSerializer.Deserialize<WebhookType>(webhookJson)
                ?? throw new ArgumentException("Invalid webhook JSON data");

            // Verify the webhook data using the SDK's synchronous method
            var verifiedData = _client.verifyPaymentWebhookData(webhookType); 
            return await Task.FromResult(verifiedData); 
        }
        catch (Exception ex)
        {
            throw new Exception($"Webhook verification failed: {ex.Message}", ex);
        }
    }
}