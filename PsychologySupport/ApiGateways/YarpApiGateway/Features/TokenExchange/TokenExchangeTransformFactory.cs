using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
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
                var statusCode = StatusCodes.Status403Forbidden;
                var title = "Hồ sơ chưa hoàn thiện";
                var detail = "Hồ sơ của bạn chưa được hoàn thiện. Vui lòng cập nhật hồ sơ để tiếp tục.";
                var errorCode = "PROFILE_INCOMPLETE";

                context.HttpContext.Response.StatusCode = statusCode;
                context.HttpContext.Response.ContentType = "application/json";

                var problemDetails = new ProblemDetails
                {
                    Title = title,
                    Detail = detail,
                    Status = statusCode,
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["errorCode"] = errorCode;
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails);
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