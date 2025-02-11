using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Profile.API.Features.PatientProfiles.DeletePatientProfile
{
    public record DeletePatientProfileRequest(Guid Id);

    public record DeletePatientProfileResponse(bool IsSuccess);
    public class DeletePatientProfileEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("patient-profile", async ([FromBody] DeletePatientProfileRequest request, ISender sender) =>
            {
                var result = await sender.Send(new DeletePatientProfileCommand(request.Id));

                var response = result.Adapt<DeletePatientProfileResponse>();

                return Results.Ok(response);
            })
        .WithName("DeletePatientProfile")
        .Produces<DeletePatientProfileResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Delete Patient Profile")
        .WithSummary("Delete Patient Profile");
        }
    }
}
