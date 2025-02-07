using Carter;
using Mapster;
using MediatR;
using Profile.API.Models;

namespace Profile.API.Features.DoctorProfiles.GetDoctorProfile;

public record GetDoctorProfileResponse(DoctorProfile DoctorProfile);

public class GetDoctorProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/doctor/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetDoctorProfileQuery(id);

                var result = await sender.Send(query);

                var response = result.Adapt<GetDoctorProfileResponse>();

                return Results.Ok(response);
            })
            .WithName("GetDoctorProfile")
            .Produces<GetDoctorProfileResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Doctor Profile")
            .WithSummary("Get Doctor Profile");
    }
}