﻿using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.UpdatePatientProfile;

public record UpdatePatientProfileRequest(UpdatePatientProfileDto PatientProfileUpdate);
public record UpdatePatientProfileResponse(Guid Id);

public class UpdatePatientProfileEndpoint : ICarterModule
{   
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patients/{id:guid}",
            async ([FromRoute] Guid id,
                   [FromBody] UpdatePatientProfileRequest request,
                   IValidator<UpdatePatientProfileDto> validator,
                   ISender sender) =>
            {
                
                
                var validationResult = await validator.ValidateAsync(request.PatientProfileUpdate);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = new UpdatePatientProfileCommand(id, request.PatientProfileUpdate);
                var result = await sender.Send(command);
                var response = result.Adapt<UpdatePatientProfileResponse>();

                return Results.Ok(response);
            })
        .WithName("UpdatePatientProfile")
        .WithTags("PatientProfiles")
        .Produces<UpdatePatientProfileResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Update Patient Profile")
        .WithSummary("Update Patient Profile");
    }
}
