using Carter;
using LifeStyles.API.Data.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity
{
    public record CreatePatientEntertainmentActivityRequest(Guid PatientProfileId, Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel);
    public record CreatePatientEntertainmentActivityResponse(Guid PatientProfileId, Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel);

    public class CreatePatientEntertainmentActivityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("patient-entertainment-activities", async ([FromBody] CreatePatientEntertainmentActivityRequest request, ISender sender) =>
            {
                var command = new CreatePatientEntertainmentActivityCommand(
                     new Models.PatientEntertainmentActivity
                     {
                         PatientProfileId = request.PatientProfileId,
                         EntertainmentActivityId = request.EntertainmentActivityId,
                         PreferenceLevel = request.PreferenceLevel
                     }
 );


                await sender.Send(command);

                var response = new CreatePatientEntertainmentActivityResponse(
                    request.PatientProfileId,
                    request.EntertainmentActivityId,
                    request.PreferenceLevel
                );

                return Results.Created($"/patient-entertainment-activities/{request.PatientProfileId}/{request.EntertainmentActivityId}", response);
            })
            .WithName("CreatePatientEntertainmentActivity")
            .Produces<CreatePatientEntertainmentActivityResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Patient Entertainment Activity")
            .WithSummary("Create Patient Entertainment Activity");
        }
    }
}
