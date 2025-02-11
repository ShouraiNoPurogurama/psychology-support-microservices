using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Dtos;

namespace Profile.API.Features.DoctorProfiles.UpdateDoctorProfile;

public record UpdateDoctorProfileRequest(DoctorProfileDto DoctorProfile);
public record UpdateDoctorProfileResponse(bool IsSuccess);


public class UpdateDoctorProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("doctor-profile", async ([FromBody]UpdateDoctorProfileRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateDoctorProfileCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateDoctorProfileResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateDoctorProfile")
            .Produces<UpdateDoctorProfileResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Doctor Profile")
            .WithSummary("Update Doctor Profile");
    }
}