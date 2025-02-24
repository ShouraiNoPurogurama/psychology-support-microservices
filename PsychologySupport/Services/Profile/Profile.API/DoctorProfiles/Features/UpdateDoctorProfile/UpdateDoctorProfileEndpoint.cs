using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.UpdateDoctorProfile;

public record UpdateDoctorProfileRequest(DoctorProfileDto DoctorProfileUpdate);
public record UpdateDoctorProfileResponse(Guid Id);

public class UpdateDoctorProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/doctors/{id:guid}", async ([FromRoute] Guid id, [FromBody] UpdateDoctorProfileRequest request, ISender sender) =>
        {
            var command = new UpdateDoctorProfileCommand(id, request.DoctorProfileUpdate);
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateDoctorProfileResponse>();
            return Results.Ok(response);
        })
        .WithName("UpdateDoctorProfile")
        .Produces<UpdateDoctorProfileResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Update Doctor Profile")
        .WithSummary("Update Doctor Profile");
    }
}
