using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetAllPatientProfiles;

public record GetAllPatientProfilesResponse(IEnumerable<GetPatientProfileDto> PatientProfileDtos);

public class GetAllPatientProfilesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetAllPatientProfilesQuery(request);

                var result = await sender.Send(query);

                var response = result.Adapt<GetAllPatientProfilesResponse>();

                return Results.Ok(response);
            })
            .WithName("GetAllPatientProfiles") 
            .Produces<GetAllPatientProfilesResponse>() 
            .ProducesProblem(StatusCodes.Status400BadRequest) 
            .ProducesProblem(StatusCodes.Status404NotFound); 
    }
}