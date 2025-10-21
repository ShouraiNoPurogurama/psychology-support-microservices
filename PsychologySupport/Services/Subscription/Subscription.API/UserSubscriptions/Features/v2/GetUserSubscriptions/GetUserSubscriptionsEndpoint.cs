using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Subscription.API.Common;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v2.GetUserSubscriptions;

public record GetUserSubscriptionsV2Request( int PageIndex = 1,
    int PageSize = 10,
    string? Search = "",
    string? SortBy = "StartDate",
    string? SortOrder = "asc",
    Guid? ServicePackageId = null,
    SubscriptionStatus? Status = null);

public record GetUserSubscriptionsV2Response(PaginatedResult<GetUserSubscriptionDto> UserSubscriptions);

public class GetUserSubscriptionsV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/me/user-subscriptions", async (
                [AsParameters] GetUserSubscriptionsV2Request request,
                ISender sender, HttpContext httpContext) =>
            {

                // Lấy patientId từ token 
                var patientIdClaim = httpContext.User.FindFirst("patientId")?.Value;
                Guid? patientId = null;
                if (!string.IsNullOrEmpty(patientIdClaim) && Guid.TryParse(patientIdClaim, out var parsedId))
                    patientId = parsedId;

                var query = new GetUserSubscriptionsQuery(
                   PageIndex: request.PageIndex,
                   PageSize: request.PageSize,
                   Search: request.Search,
                   SortBy: request.SortBy,
                   SortOrder: request.SortOrder,
                   ServicePackageId: request.ServicePackageId,
                   PatientId: patientId,
                   Status: request.Status
               );

                var result = await sender.Send(query);
                var response = result.Adapt<GetUserSubscriptionsV2Response>();

                return Results.Ok(response);
            })
            .WithName("GetUserSubscriptions v2")
            .WithTags("UserSubscriptions Version 2")
            .Produces<GetUserSubscriptionsV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All User Subscriptions")
            .WithSummary("Get All User Subscriptions");
    }
}