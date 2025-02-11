using Carter;
using Mapster;
using MediatR;
using Profile.API.Models;

namespace Profile.API.Features.PatientProfiles.GetPatientProfile
{
    public record GetPatientProfileResponse(PatientProfile PatientProfile);
    public class GetPatientProfileEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/patient/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetPatientProfileQuery(id);

                var result = await sender.Send(query);

                var response = result.Adapt<GetPatientProfileResponse>();

                return Results.Ok(response);
            })
            .WithName("GetPatientProfile")
            .Produces<GetPatientProfileResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Profile")
            .WithSummary("Get Patient Profile");
        }
    }
}
