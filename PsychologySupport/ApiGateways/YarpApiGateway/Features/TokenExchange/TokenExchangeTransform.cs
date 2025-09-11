// using YarpApiGateway.Features.TokenExchange.Contracts;
//
// namespace YarpApiGateway.Features.TokenExchange;
//
// using Yarp.ReverseProxy.Transforms;
//
// public class TokenExchangeTransform : RequestTransform
// {
//     private readonly ITokenExchangeService _tokenExchangeService;
//
//     public TokenExchangeTransform(ITokenExchangeService tokenExchangeService)
//     {
//         _tokenExchangeService = tokenExchangeService;
//     }
//
//     public async ValueTask ApplyAsync(RequestTransformContext context)
//     {
//         var originalToken = context.HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
//         if (string.IsNullOrEmpty(originalToken)) return;
//
//         //Lấy audience từ ClusterId của YARP
//         var destinationAudience = context.Cluster.ClusterId;
//         
//         var newScopedToken = await _tokenExchangeService.ExchangeTokenAsync(originalToken, destinationAudience);
//         if (string.IsNullOrEmpty(newScopedToken))
//         {
//             context.HttpContext.Response.StatusCode = 403; //Forbidden
//             return;
//         }
//
//         context.ProxyRequest.Headers.Authorization = new("Bearer", newScopedToken);
//     }
// }