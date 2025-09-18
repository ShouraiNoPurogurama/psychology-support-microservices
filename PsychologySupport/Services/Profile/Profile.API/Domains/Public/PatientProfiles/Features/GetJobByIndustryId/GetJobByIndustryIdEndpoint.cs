using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetJobByIndustryId
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
                .WithTags("Jobs")
                .Produces<GetJobByIndustryIdResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Retrieves all jobs associated with the specified industry. Requires 'User' or 'Admin' role.")
                .WithSummary("Get jobs by industry ID");
        }
    }

}
