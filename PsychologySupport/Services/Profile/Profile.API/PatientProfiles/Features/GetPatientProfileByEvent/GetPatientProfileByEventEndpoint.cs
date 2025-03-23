using BuildingBlocks.Enums;
using Carter;
using Mapster;

namespace Profile.API.PatientProfiles.Features.GetPatientProfileByEvent;

public record GetPatientProfileByEventRequest(Guid DoctorId, Guid? UserId = null);

public record GetDoctorProfileByEventResponse(
    bool PatientExists,
    Guid Id,
    string FullName,
    UserGender Gender,
    string? Allergies,
    string PersonalityTraits,
    string Address,
    string PhoneNumber,
    string Email);

public class GetPatientProfileByEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/get-by-event", async ([AsParameters] GetPatientProfileByEventRequest request, ISender sender) =>
        {
            var result = await sender.Send(new GetPatientProfileByEventQuery(request.DoctorId, request.UserId));

            var response = result.Adapt<GetDoctorProfileByEventResponse>();
            
            return Results.Ok(response);
        })
        .WithName("GetPatientProfileByEvent")
        .Produces<GetDoctorProfileByEventResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("PatientProfiles")
        .WithDescription("Get patient profile by event");
    }
}