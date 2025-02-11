using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Profile.API.Features.DoctorProfiles.DeleteDoctorProfile;

public record DeleteDoctorProfileRequest(Guid Id);

public record DeleteDoctorProfileResponse(bool IsSuccess);

public class DeleteDoctorProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("doctor-profile", async ([FromBody] DeleteDoctorProfileRequest request, ISender sender) =>
        {
            var result = await sender.Send(new DeleteDoctorProfileCommand(request.Id));

            var response = result.Adapt<DeleteDoctorProfileResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteDoctorProfile")
        .Produces<DeleteDoctorProfileResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Delete Doctor Profile")
        .WithSummary("Delete Doctor Profile");
    }
}