using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using Media.Application.Features.Media.Dtos;
using Media.Application.Features.Media.Queries;

namespace Media.API.Endpoints;

public record GetMediaRequest(Guid MediaId);

public record GetMediaResponse(MediaDto Media);

public class GetMediaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/media/{mediaId:guid}", async (
                [AsParameters] GetMediaRequest request,
                ISender sender,
                HttpContext httpContext) =>
        {
            // Authorization check
            if (!httpContext.User.IsInRole("User") && !httpContext.User.IsInRole("Admin"))
                throw new ForbiddenException();

            var query = request.Adapt<GetMediaQuery>();

            var result = await sender.Send(query);

            var response = new GetMediaResponse(result.Media);

            return Results.Ok(response);
        })
         .WithName("GetMedia")
         .WithTags("Media")
         .Produces<GetMediaResponse>()
         .ProducesProblem(StatusCodes.Status400BadRequest)
         .ProducesProblem(StatusCodes.Status404NotFound)
         .WithDescription("GetMedia")
         .WithSummary("GetMedia");
    }
}