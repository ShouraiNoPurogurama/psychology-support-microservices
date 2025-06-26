using Carter;
using Mapster;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.GetDoctorSchedule
{
    public record GetDoctorScheduleRequest(Guid DoctorId, DateOnly Date);
    public record GetDoctorScheduleResponse(List<TimeSlotDto> TimeSlots);

    public class GetDoctorScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/doctor-schedule/{doctorId:guid}/{date}", async (Guid doctorId, DateOnly date, ISender sender) =>
            {
                var query = new GetDoctorScheduleQuery(doctorId, date);
                var result = await sender.Send(query);
                return Results.Ok(result.Adapt<GetDoctorScheduleResponse>());
            })
                .RequireAuthorization(policy => policy.RequireRole("Doctor", "Admin","Manager"))
                .WithName("GetDoctorSchedule")
                .WithTags("Doctor Schedule")
                .Produces<GetDoctorScheduleResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get Doctor Schedule")
                .WithSummary("Get Doctor Schedule");
        }
    }
}
