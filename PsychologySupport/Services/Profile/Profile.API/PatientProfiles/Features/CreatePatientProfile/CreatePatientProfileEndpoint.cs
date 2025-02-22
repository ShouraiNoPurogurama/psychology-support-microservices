using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.CreatePatientProfile
{
    public record CreatePatientProfileRequest(PatientProfileCreate PatientProfileCreate);
    public record CreatePatientProfileResponse(Guid Id);

    public class CreatePatientProfileEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("patient-profiles", async ([FromBody] CreatePatientProfileRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreatePatientProfileCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreatePatientProfileResponse>();

                return Results.Created($"/patient-profiles/{response.Id}", response);
            })
            .WithName("CreatePatientProfile")
            .Produces<CreatePatientProfileResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Patient Profile")
            .WithSummary("Create Patient Profile");
        }
    }
}
