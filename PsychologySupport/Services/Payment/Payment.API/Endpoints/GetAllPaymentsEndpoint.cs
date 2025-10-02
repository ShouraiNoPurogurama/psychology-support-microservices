using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Payment.API.Common;
using Payment.Application.Payments.Dtos;
using Payment.Application.Payments.Queries;
using Payment.Domain.Enums;

namespace Payment.API.Endpoints
{
    public record GetAllPaymentsRequest(
        int PageIndex = 1,
        int PageSize = 10,
        Guid? PatientProfileId = null,
        PaymentStatus? Status = null,
        DateOnly? CreatedAt = null,
        PaymentType? PaymentType = null,
        string? SortOrder = "desc"
    );
    public record GetAllPaymentsResponse(PaginatedResult<PaymentDto> Payments);

    public class GetAllPaymentsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/payments", async ([AsParameters] GetAllPaymentsRequest request,
                ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (request.PatientProfileId is { } patientId)
                    {
                        if (!AuthorizationHelpers.CanViewPatientProfile(patientId, httpContext.User) && !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                            throw new ForbiddenException();
                    }

                    var query = request.Adapt<GetAllPaymentsQuery>();
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetAllPaymentsResponse>();
                    return Results.Ok(response);
                })
                .RequireAuthorization(policy => policy.RequireRole("User","Admin","Manager"))
                .WithName("GetAllPayments")
                .WithTags("Dashboard")
                .Produces<GetAllPaymentsResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get All Payments")
                .WithSummary("Get All Payments");
        }
    }
}
