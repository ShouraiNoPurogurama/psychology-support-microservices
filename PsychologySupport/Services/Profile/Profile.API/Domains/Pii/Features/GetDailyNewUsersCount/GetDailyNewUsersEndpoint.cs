using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.Pii.Dtos;

namespace Profile.API.Domains.Pii.Features.GetDailyNewUsersCount;

public record GetDailyNewUserResponse(DailyNewUserStatsDto? DailyNewUserStats);

public class GetDailyNewUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/dashboard/daily-new-users", async (
                [FromQuery] int? year,
                [FromQuery] int? month, 
                ISender sender) =>
            {
                var query = new GetDailyNewUsersQuery(year, month);

                var result = await sender.Send(query);

                var response = result.Adapt<GetDailyNewUserResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("GetDailyNewUser")
            .WithTags("Dashboard")
            .Produces<GetDailyNewUserResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}