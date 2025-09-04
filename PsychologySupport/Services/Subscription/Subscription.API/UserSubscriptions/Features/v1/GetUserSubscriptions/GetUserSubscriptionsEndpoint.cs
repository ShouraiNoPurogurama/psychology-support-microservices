using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Subscription.API.Common;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v1.GetUserSubscriptions;

public record GetUserSubscriptionsRequest( int PageIndex = 1,
    int PageSize = 10,
    string? Search = "",
    string? SortBy = "StartDate",
    string? SortOrder = "asc",
    Guid? ServicePackageId = null,
    Guid? PatientId = null, 
    SubscriptionStatus? Status = null);

public record GetUserSubscriptionsResponse(PaginatedResult<GetUserSubscriptionDto> UserSubscriptions);

public class GetUserSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-subscriptions", async (
                [AsParameters] GetUserSubscriptionsRequest request,
                ISender sender, HttpContext httpContext) =>
            {
                if(request.PatientId.HasValue && !(AuthorizationHelpers.CanViewPatientProfile(request.PatientId.Value ,httpContext.User) && AuthorizationHelpers.IsExclusiveAccess(httpContext.User)))
                    throw new ForbiddenException();
                
                // Authorization check
                if (!(AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User) || AuthorizationHelpers.IsExclusiveAccess(httpContext.User)))
                    throw new ForbiddenException();

                var query = request.Adapt<GetUserSubscriptionsQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetUserSubscriptionsResult>();

                return Results.Ok(response);
            })
            .WithName("GetUserSubscriptions")
            .WithTags("UserSubscriptions")
            .Produces<GetUserSubscriptionsResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All User Subscriptions")
            .WithSummary("Get All User Subscriptions");
    }
}