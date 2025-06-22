// using Carter;
// using MediatR;
//
// namespace LifeStyles.API.Features.CurrentEmotion.GetCurrentEmotion;
//
// public record GetCurrentEmotionResponse(Guid Id, Guid PatientProfileId, DateTimeOffset LogDate, string? Emotion1, string? Emotion2);
//
// public class GetCurrentEmotionEndpoint : ICarterModule
// {
//     public void AddRoutes(IEndpointRouteBuilder app)
//     {
//         app.MapGet("/current-emotions/by-date",
//             async (Guid patientProfileId, DateTime date, ISender sender) =>
//             {
//                 var query = new GetCurrentEmotionQuery(patientProfileId, date);
//                 var result = await sender.Send(query);
//
//                 var response = new GetCurrentEmotionResponse(
//                     result.Id,
//                     result.PatientProfileId,
//                     result.LogDate,
//                     result.Emotion1,
//                     result.Emotion2
//                 );
//
//                 return Results.Ok(response);
//             })
//             .WithName("GetCurrentEmotionByDate")
//             .WithTags("CurrentEmotion")
//             .WithSummary("Get current emotion log by exact date")
//             .WithDescription("GetCurrentEmotionByDate")
//             .Produces<GetCurrentEmotionResponse>()
//             .ProducesProblem(StatusCodes.Status404NotFound)
//             .ProducesProblem(StatusCodes.Status400BadRequest);
//     }
// }
