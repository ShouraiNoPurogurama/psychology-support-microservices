using Carter;
using Mapster;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetDoctorProfile;

public record GetDoctorProfileRequest(Guid Id);

public record GetDoctorProfileResponse(DoctorProfileDto DoctorProfileDto);

public class GetDoctorProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/doctors/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetDoctorProfileQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetDoctorProfileResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Doctor","Manager"))
            .WithName("GetDoctorProfile")
            .WithTags("DoctorProfiles")
            .Produces<GetDoctorProfileResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get Doctor Profile")
            .WithSummary("Get Doctor Profile");
    }
}