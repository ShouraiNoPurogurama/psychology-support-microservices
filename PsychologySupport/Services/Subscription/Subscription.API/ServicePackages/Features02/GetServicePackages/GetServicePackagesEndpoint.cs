using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.ServicePackages.Dtos;
using Subscription.API.ServicePackages.Features02.GetServicePackages;

namespace Subscription.API.ServicePackages.Features02.GetServicePackagesV2;

public record GetServicePackagesV2Request(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    bool? Status = null
);

public record GetServicePackagesV2Response(PaginatedResult<ServicePackageDto> ServicePackages);

public class GetServicePackagesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/service-packages", async (
            [AsParameters] GetServicePackagesV2Request request,
            ISender sender,
            HttpContext httpContext) =>
        {
            // Lấy patientId từ token
            var patientIdClaim = httpContext.User.FindFirst("patientId")?.Value;
            Guid? patientId = null;
            if (!string.IsNullOrEmpty(patientIdClaim) && Guid.TryParse(patientIdClaim, out var parsedId))
                patientId = parsedId;

            // Tạo query
            var query = new GetServicePackagesQuery(
                PageIndex: request.PageIndex,
                PageSize: request.PageSize,
                Search: request.Search,
                Status: request.Status,
                PatientId: patientId
            );

            var result = await sender.Send(query);
            var response = new GetServicePackagesV2Response(result.ServicePackages);

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetServicePackages v2")
        .WithTags("ServicePackages Version 2")
        .Produces<GetServicePackagesV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Retrieve service packages with optional search and status filter")
        .WithSummary("Get Service Packages");
    }
}
