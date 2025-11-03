using BuildingBlocks.Messaging.Events.Queries.Profile;
using Profile.API.Domains.Public.PatientProfiles.Features.GetPatientProfile;

namespace Profile.API.Domains.Public.PatientProfiles.EventHandlers;

public class AggregatePatientProfileRequestHandler(ISender sender, ILogger<AggregatePatientProfileRequestHandler> logger) : IConsumer<AggregatePatientProfileRequest>
{
    public async Task Consume(ConsumeContext<AggregatePatientProfileRequest> context)
    {
        logger.LogInformation("Processing AggregatePatientProfileRequest for PatientProfileId: {PatientProfileId}", context.Message.PatientProfileId);
        
        var query = new GetPatientProfileQuery(context.Message.PatientProfileId);
        
        var result = await sender.Send(query, context.CancellationToken);
        
        var response = MapToAggregatePatientProfileResponse(result);
        
        await context.RespondAsync(response);
        
        logger.LogInformation("Successfully aggregated patient profile for PatientProfileId: {PatientProfileId}", context.Message.PatientProfileId);
    }
    
    private static AggregatePatientProfileResponse MapToAggregatePatientProfileResponse(GetPatientProfileResult result)
    {
        var dto = result.PatientProfileDto;

        var response = new AggregatePatientProfileResponse(
            FullName: dto.FullName,
            Gender: dto.Gender.ToString(),
            Allergies: dto.Allergies,
            PersonalityTraits: dto.PersonalityTraits.ToString(),
            BirthDate: dto.BirthDate,
            JobTitle: dto.Job?.JobTitle ?? string.Empty,
            EducationLevel: dto.Job?.EducationLevel.ToString() ?? string.Empty,
            IndustryName: dto.Job?.Industry.IndustryName ?? string.Empty
        );
        
        return response;
    }
}