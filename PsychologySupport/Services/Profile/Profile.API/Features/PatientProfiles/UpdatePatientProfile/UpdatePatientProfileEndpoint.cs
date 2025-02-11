using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Dtos;

namespace Profile.API.Features.PatientProfiles.UpdatePatientProfile
{
    public record UpdatePatientProfileRequest(PatientProfileDto PatientProfile);
    public record UpdatePatientProfileResponse(bool IsSuccess);

    public class UpdatePatientProfileEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("patient-profile", async ([FromBody] UpdatePatientProfileRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdatePatientProfileCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdatePatientProfileResponse>();
                return Results.Ok(response);
            })
                .WithName("UpdatePatientProfile")
                .Produces<UpdatePatientProfileResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Update Patient Profile")
                .WithSummary("Update Patient Profile");
        }
    }
}

