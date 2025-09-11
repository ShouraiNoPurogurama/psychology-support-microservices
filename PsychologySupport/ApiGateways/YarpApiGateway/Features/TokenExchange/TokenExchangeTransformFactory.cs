using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;
using YarpApiGateway.Features.TokenExchange.Contracts;

namespace YarpApiGateway.Features.TokenExchange;

public class TokenExchangeTransformFactory(ILogger<TokenExchangeTransformFactory> logger) : ITransformFactory
{
    public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        if (transformValues.TryGetValue("TokenExchange", out var value))
        {
            if (!string.IsNullOrEmpty(value) && value != "true" && value != "enabled")
            {
                context.Errors.Add(new ArgumentException("Không thể xử lý yêu cầu. Vui lòng thử lại sau."));
                logger.LogError("TokenExchange transform value must be 'true' or 'enabled'");
                return true;
            }

            //Check if the cluster has a valid ID for token exchange
            if (string.IsNullOrEmpty(context.Route.ClusterId))
            {
                context.Errors.Add(new ArgumentException("Không thể xử lý yêu cầu. Vui lòng thử lại sau."));
                logger.LogError("TokenExchange transform requires a valid ClusterId");
                return true;
            }

            return true;
        }

        return false; 
    }

    public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        if (transformValues.TryGetValue("TokenExchange", out var _))
        {
            context.AddRequestTransform(transformContext =>
            {
                var tokenExchangeService = transformContext.HttpContext.RequestServices
                    .GetRequiredService<ITokenExchangeService>();

                return ApplyTokenExchangeAsync(transformContext, tokenExchangeService, context.Route.ClusterId);
            });
            return true;
        }

        return false;
    }

    private async ValueTask ApplyTokenExchangeAsync(
        RequestTransformContext context,
        ITokenExchangeService tokenExchangeService,
        string clusterId)
    {
        try
        {
            // Extract Bearer token from Authorization header
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return;

            var originalToken = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(originalToken)) 
                return;
            
            logger.LogInformation("*** originalToken = {Token}", originalToken);

            var destinationAudience = clusterId;

            var newScopedToken = await tokenExchangeService.ExchangeTokenAsync(originalToken, destinationAudience);
            if (string.IsNullOrEmpty(newScopedToken))
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            
            logger.LogInformation("*** newScopedToken = {NewToken}", newScopedToken);

            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newScopedToken);
        }
        catch (Exception)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}