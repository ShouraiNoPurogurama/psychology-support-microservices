using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.RegisterDoctorBusyAllDay
{
    public record RegisterDoctorBusyAllDayRequest(RegisterDoctorBusyAllDayDto DoctorBusyDto);
    public record RegisterDoctorBusyAllDayResponse(bool Success);

    public class RegisterDoctorBusyAllDayEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/doctor-busy", async (
                RegisterDoctorBusyAllDayRequest request,
                IValidator<RegisterDoctorBusyAllDayDto> validator, 
                ISender sender) =>
            {
                var validationResult = await validator.ValidateAsync(request.DoctorBusyDto);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
   
                var command = new RegisterDoctorBusyAllDayCommand(request.DoctorBusyDto);
                var result = await sender.Send(command);

                return Results.Ok(result.Adapt<RegisterDoctorBusyAllDayResponse>());
            })
            .RequireAuthorization(policy => policy.RequireRole("Doctor", "Admin"))
            .WithName("RegisterDoctorBusyAllDay")
            .WithTags("Doctor Schedule")
            .Produces<RegisterDoctorBusyAllDayResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("RegisterDoctorBusyAllDay")
            .WithSummary("Register Doctor Busy All Day");
        }
    }
}
