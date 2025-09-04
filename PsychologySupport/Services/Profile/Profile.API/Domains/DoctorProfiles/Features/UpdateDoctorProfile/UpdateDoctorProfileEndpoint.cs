using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.DoctorProfiles.Dtos;

namespace Profile.API.Domains.DoctorProfiles.Features.UpdateDoctorProfile;

public record UpdateDoctorProfileRequest(UpdateDoctorProfileDto DoctorProfileUpdate);
public record UpdateDoctorProfileResponse(Guid Id);

public class UpdateDoctorProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/doctors/{id:guid}",
            async ([FromRoute] Guid id,
                   [FromBody] UpdateDoctorProfileRequest request,
                   IValidator<UpdateDoctorProfileDto> validator, 
                   ISender sender) =>
            {
                var validationResult = await validator.ValidateAsync(request.DoctorProfileUpdate);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = new UpdateDoctorProfileCommand(id, request.DoctorProfileUpdate);
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateDoctorProfileResponse>();

                return Results.Ok(response);
            })
         .RequireAuthorization(policy => policy.RequireRole("Doctor", "Admin"))
        .WithName("UpdateDoctorProfile")
        .WithTags("DoctorProfiles")
        .Produces<UpdateDoctorProfileResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Update Doctor Profile")
        .WithSummary("Update Doctor Profile");
    }
}
