﻿using BuildingBlocks.Enums;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity;

public record CreatePatientEntertainmentActivityRequest(Guid PatientProfileId, List<EntertainmentPreference> Activities);

public record EntertainmentPreference(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientEntertainmentActivityResponse(Guid PatientProfileId, List<EntertainmentPreference> Activities);

public class CreatePatientEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("patient-entertainment-activities",
                async ([FromBody] CreatePatientEntertainmentActivityRequest request, ISender sender) =>
                {
                    var command = new CreatePatientEntertainmentActivityCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.EntertainmentActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new CreatePatientEntertainmentActivityResponse(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Created($"/patient-entertainment-activities/{request.PatientProfileId}", response);
                })
            .WithName("CreatePatientEntertainmentActivities")
            .WithTags("PatientEntertainmentActivities")
            .Produces<CreatePatientEntertainmentActivityResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Patient Entertainment Activities")
            .WithSummary("Create Patient Entertainment Activities");
    }
}