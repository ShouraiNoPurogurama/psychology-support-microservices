using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.Pii.Features.GetDailyNewUsersCount;

namespace Profile.API.Domains.Pii.Features.GetTotalUsersCount;

public record GetTotalUsersCountResponse(
    DateOnly Date,
    int TotalUsersCount
);

public class GetTotalUsersCountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/dashboard/total-users-count", async (
                [FromQuery] int? year,
                [FromQuery] int? month, 
                ISender sender) =>
            {
                var query = new GetTotalUsersCountQuery(year, month);

                var result = await sender.Send(query);

                var response = result.Adapt<GetTotalUsersCountResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("GetTotalUsersCount")
            .WithTags("Dashboard")
            .Produces<GetTotalUsersCountResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}