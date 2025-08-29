using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.PatientProfiles.Dtos;

namespace Profile.API.Domains.PatientProfiles.Features.CreatePatientProfile;

public record CreatePatientProfileRequest(CreatePatientProfileDto PatientProfileCreate);

public record CreatePatientProfileResponse(Guid Id);

public class CreatePatientProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("patients", async ([FromBody] CreatePatientProfileRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreatePatientProfileCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreatePatientProfileResponse>();

                return Results.Created($"/patient-profiles/{response.Id}", response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientProfile")
            .WithTags("PatientProfiles")
            .Produces<CreatePatientProfileResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Patient Profile")
            .WithSummary("Create Patient Profile");
    }
}