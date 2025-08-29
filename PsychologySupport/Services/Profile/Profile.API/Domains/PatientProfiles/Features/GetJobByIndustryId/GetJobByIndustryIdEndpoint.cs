using Carter;
using Mapster;
using Profile.API.Domains.PatientProfiles.Dtos;

namespace Profile.API.Domains.PatientProfiles.Features.GetJobByIndustryId
{
    public record GetJobByIndustryIdResponse(List<JobDto> Jobs);

    public class GetJobByIndustryIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/industries/{industryId:guid}/jobs", async (Guid industryId, ISender sender) =>
            {
                var query = new GetJobByIndustryIdQuery(industryId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetJobByIndustryIdResponse>();
                return Results.Ok(response);
            })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("GetJobByIndustryId")
                .WithTags("PatientProfiles")
                .Produces<GetJobByIndustryIdResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Get all jobs by IndustryId")
                .WithSummary("Get all jobs by IndustryId");
        }
    }

}
