using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Profile.API.DoctorProfiles.Dtos;
using System;
using System.Threading.Tasks;

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
            .WithName("GetDoctorProfile")
            .Produces<GetDoctorProfileResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
