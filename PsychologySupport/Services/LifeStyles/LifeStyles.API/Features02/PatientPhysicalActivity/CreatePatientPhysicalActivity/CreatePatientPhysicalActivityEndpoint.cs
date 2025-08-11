using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientPhysicalActivity.CreatePatientPhysicalActivity;

public record CreatePatientPhysicalActivityV2Request(
    Guid PatientProfileId,
    List<PhysicalPreferenceV2> Activities
);

public record PhysicalPreferenceV2(Guid PhysicalActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientPhysicalActivityV2Response(
    Guid PatientProfileId,
    List<PhysicalPreferenceV2> Activities
);

public class CreatePatientPhysicalActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("v2/me/physicalActivities",
            async (HttpContext httpContext,
                   [FromBody] CreatePatientPhysicalActivityV2Request request,
                   ISender sender) =>
            {
                if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = new CreatePatientPhysicalActivityV2Command(
                    request.PatientProfileId,
                    request.Activities.Select(a => (a.PhysicalActivityId, a.PreferenceLevel)).ToList()
                );

                await sender.Send(command);

                var response = command.Adapt<CreatePatientPhysicalActivityV2Response>();

                return Results.Created($"/v2/patient-physical-activities/{request.PatientProfileId}", response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientPhysicalActivity v2")
            .WithTags("PatientPhysicalActivities Version 2")
            .Produces<CreatePatientPhysicalActivityV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create multiple Patient Physical Activities - V2")
            .WithSummary("Create multiple Patient Physical Activities - V2");
    }
}