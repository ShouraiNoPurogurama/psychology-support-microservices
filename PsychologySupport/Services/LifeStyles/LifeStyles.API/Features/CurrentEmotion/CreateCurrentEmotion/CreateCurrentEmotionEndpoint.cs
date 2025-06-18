using BuildingBlocks.Enums;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.CurrentEmotion.CreateCurrentEmotion;

public record CreateCurrentEmotionRequest(
    Guid PatientProfileId,
    Emotion? Emotion1,
    Emotion? Emotion2
);

public record CreateCurrentEmotionResponse(Guid Id);

public class CreateCurrentEmotionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/current-emotions",
            async ([FromBody] CreateCurrentEmotionRequest request, ISender sender) =>
            {
                var command = new CreateCurrentEmotionCommand(
                    request.PatientProfileId,
                    request.Emotion1,
                    request.Emotion2
                );

                var result = await sender.Send(command);

                var response = new CreateCurrentEmotionResponse(result.Id);

                return Results.Created($"/current-emotions/{result.Id}", response);
            })
            .WithName("CreateCurrentEmotion")
            .WithTags("CurrentEmotion")
            .WithSummary("Create a Current Emotion entry")
            .WithDescription("Logs current emotional state for a patient.")
            .Produces<CreateCurrentEmotionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
